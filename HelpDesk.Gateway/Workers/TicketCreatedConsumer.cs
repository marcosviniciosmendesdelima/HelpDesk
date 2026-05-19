using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using HelpDesk.Gateway.Services; // Reconhece a camada de cache (Etapa 9)
using HelpDesk.Gateway.Hubs;     // <-- ADICIONADO: Reconhece o seu TicketHub (Etapa 10.1) 🚀
using Microsoft.AspNetCore.SignalR; // <-- ADICIONADO: Permite usar o contexto do SignalR (Etapa 10.1) 🚀

namespace HelpDesk.Gateway.Workers
{
    public class TicketCreatedConsumer : BackgroundService
    {
        private readonly ITicketCacheService _cacheService; // Serviço de cache
        private readonly IHubContext<TicketHub> _hubContext; // <-- ADICIONADO: Contexto do SignalR 🚀
        private readonly string _connectionString = "Host=helpdesk-db;Port=5432;Database=postgres;Username=postgres;Password=SenhaForte123;";
        private const string QueueName = "ticket_created";

        // Construtor atualizado recebendo o Cache e o Hub do SignalR via Injeção de Dependência
        public TicketCreatedConsumer(ITicketCacheService cacheService, IHubContext<TicketHub> hubContext)
        {
            _cacheService = cacheService;
            _hubContext = hubContext; // <-- ADICIONADO 🚀
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConnection? connection = null;
            IModel? channel = null;

            // Loop de conexão ao RabbitMQ
            while (!stoppingToken.IsCancellationRequested && connection == null)
            {
                try
                {
                    var factory = new ConnectionFactory() { HostName = "rabbitmq", DispatchConsumersAsync = true };
                    connection = factory.CreateConnection();
                    channel = connection.CreateModel();
                    Console.WriteLine(" [*] SUCESSO: Gateway conectado ao RabbitMQ!");
                }
                catch
                {
                    Console.WriteLine(" [!] Aguardando RabbitMQ ficar pronto...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            if (channel == null) return;

            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[*] Mensagem recebida: {message}");

                    using var doc = JsonDocument.Parse(message);
                    var root = doc.RootElement;

                    // Extração de dados com tratamento para campos nulos
                    var id = root.TryGetProperty("id", out var idProp) ? idProp.GetGuid() : Guid.NewGuid();
                    var titulo = root.GetProperty("titulo").GetString() ?? "Sem Título";
                    var descricao = root.GetProperty("descricao").GetString() ?? "Sem Descrição";
                    var status = root.TryGetProperty("status", out var sProp) ? sProp.GetString() : "Aberto";
                    
                    string prioridade = "Média";
                    if (root.TryGetProperty("prioridade", out var p)) prioridade = p.GetString() ?? "Média";
                    else if (root.TryGetProperty("prioridade_valor", out var pv)) prioridade = pv.GetString() ?? "Média";

                    var ticketData = new {
                        id = id,
                        titulo = titulo,
                        descricao = descricao,
                        status = status,
                        prioridade = prioridade
                    };

                    using var db = new NpgsqlConnection(_connectionString);
                    
                    // Comando SQL para inserir ou ignorar se o ID já existir
                    var sql = @"INSERT INTO TicketsRead (id, titulo, descricao, status, prioridade)
                                VALUES (@id, @titulo, @descricao, @status, @prioridade)
                                ON CONFLICT (id) DO NOTHING;";

                    await db.ExecuteAsync(sql, ticketData);
                    Console.WriteLine($" [Gateway] SUCESSO: Ticket '{titulo}' sincronizado com o banco de leitura!");

                    // INVALIDAÇÃO DO CACHE: Remove a chave antiga do Redis na hora para manter a consistência de dados
                    await _cacheService.RemoveTicketCacheAsync("tickets:all");
                    Console.WriteLine(" [Redis] Cache de tickets limpo e invalidado devido a uma nova insercao via RabbitMQ.");

                    // ============================================================================
                    // 🚀 EXECUTANDO A ETAPA 10.1: DISPARO SERVER-PUSH EM TEMPO REAL VIA WEBSOCKET
                    // ============================================================================
                    
                    // Descobre o nome exato da sala/grupo usando a regra estática que você definiu no TicketHub.cs
                    string salaDoTicket = TicketHub.ObterNomeDoGrupo(id);

                    // Envia os dados do ticket de forma assíncrona apenas para quem estiver ouvindo essa sala específica
                    await _hubContext.Clients.Group(salaDoTicket).SendAsync("OnTicketStatusChanged", new {
                        ticketId = id,
                        titulo = titulo,
                        status = status,
                        prioridade = prioridade,
                        notificadoEm = DateTime.UtcNow,
                        mensagem = "O painel deste ticket foi atualizado reativamente em tempo real!"
                    });

                    Console.WriteLine($" [SignalR] Notificação em tempo real enviada com sucesso para a sala '{salaDoTicket}'.");
                    // ============================================================================
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" [!] ERRO AO PROCESSAR: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}