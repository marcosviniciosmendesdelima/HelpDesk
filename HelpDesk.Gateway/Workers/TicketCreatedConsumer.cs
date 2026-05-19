using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using HelpDesk.Gateway.Services; // Adicionado para reconhecer a camada de cache

namespace HelpDesk.Gateway.Workers
{
    public class TicketCreatedConsumer : BackgroundService
    {
        private readonly ITicketCacheService _cacheService; // Injetando o serviço de cache
        private readonly string _connectionString = "Host=helpdesk-db;Port=5432;Database=postgres;Username=postgres;Password=SenhaForte123;";
        private const string QueueName = "ticket_created";

        // Construtor atualizado para receber a Injeção de Dependência do Cache
        public TicketCreatedConsumer(ITicketCacheService cacheService)
        {
            _cacheService = cacheService;
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