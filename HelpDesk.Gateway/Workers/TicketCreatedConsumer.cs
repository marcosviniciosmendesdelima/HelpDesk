using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR; 
using HelpDesk.Gateway.Hubs;       
using Microsoft.Extensions.DependencyInjection; 
using Serilog;

namespace HelpDesk.Gateway.Workers;

public class TicketCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;
    private readonly IServiceProvider _serviceProvider; 

    public TicketCreatedConsumer(IConfiguration config, IServiceProvider serviceProvider)
    {
        _config = config;
        _serviceProvider = serviceProvider; 

        _connectionString = _config.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string não encontrada.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Tratamento seguro de tipo para evitar o aviso CS8600 de conversão nula
        string? configHost = _config["RabbitMQ:Host"];
        string rabbitHost = !string.IsNullOrEmpty(configHost) ? configHost : "localhost";
        
        // Se no arquivo de configuração estiver o nome do container, mas você estiver testando local fora do Docker
        if (rabbitHost == "helpdesk-rabbitmq" && !Environment.GetEnvironmentVariables().Contains("DOTNET_RUNNING_IN_CONTAINER"))
        {
            rabbitHost = "localhost"; // Redireciona para o localhost do Mac para o teste funcionar localmente
        }

        var factory = new ConnectionFactory() { HostName = rabbitHost };
        IConnection? connection = null;
        int tentativas = 0;

        Log.Information("Iniciando escuta do consumidor. Tentando conectar ao RabbitMQ no host: {RabbitHost}", rabbitHost);

        // Laço de tentativas manual para aguardar o broker de mensageria subir
        while (connection == null && !stoppingToken.IsCancellationRequested && tentativas < 10)
        {
            try
            {
                tentativas++;
                connection = factory.CreateConnection();
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Log.Warning(ex, "Aguardando RabbitMQ ficar online. Tentativa {Tentativa}/10 no host {Host}", tentativas, rabbitHost);
                await Task.Delay(5000, stoppingToken); 
            }
        }

        // Defesa crucial contra o NullReferenceException capturado no teste anterior
        if (connection == null)
        {
            Log.Fatal("Não foi possível estabelecer conexão com o RabbitMQ após 10 tentativas. O Worker será encerrado.");
            return; // Aborta a execução do serviço de forma segura sem derrubar a aplicação inteira
        }

        using var currentConnection = connection;
        using var channel = currentConnection.CreateModel();

        // Declaração da fila baseada no projeto original
        channel.QueueDeclare(queue: "fila_tickets", durable: true, exclusive: false, autoDelete: false);
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Log.Information("Nova mensagem capturada na fila_tickets: {MensagemCorpo}", message);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var ticketData = JsonSerializer.Deserialize<TicketReadModel>(message, options);

                if (ticketData != null)
                {
                    Log.Debug("Conversão de JSON realizada com sucesso para o Ticket ID: {TicketId}", ticketData.Id);

                    // Disparo do SignalR via Escopo isolado para garantir a estabilidade do tempo real
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TicketsHub>>();
                        
                        var payloadNotificacao = new Dictionary<string, string>
                        {
                            { "ticketId", ticketData.Id ?? "Sem ID" },
                            { "status", ticketData.Status ?? "Sem Status" }
                        };

                        await hubContext.Clients.All.SendAsync("OnTicketStatusChanged", payloadNotificacao);
                        Log.Information("Notificação de tempo real via WebSocket enviada para o navegador para o Ticket: {TicketId}", ticketData.Id);
                    }

                    // Gravação persistente no banco relacional PostgreSQL
                    try
                    {
                        using var db = new NpgsqlConnection(_connectionString);
                        var sql = @"INSERT INTO ""TicketsRead"" (""Id"", ""Titulo"", ""Descricao"", ""Prioridade"", ""Status"") 
                                    VALUES (@Id, @Titulo, @Descricao, @Prioridade, @Status)";
                        
                        await db.ExecuteAsync(sql, ticketData);
                        Log.Information("Ticket {TicketId} sincronizado com sucesso na tabela TicketsRead", ticketData.Id);
                    }
                    catch (Exception dbEx)
                    {
                        // Log estruturado do erro de banco sem derrubar a transmissão em tempo real do WebSocket
                        Log.Error(dbEx, "Falha na persistência de dados no PostgreSQL para o Ticket {TicketId}", ticketData.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro crítico inesperado durante o processamento da mensagem do RabbitMQ");
            }
        };

        channel.BasicConsume(queue: "fila_tickets", autoAck: true, consumer: consumer);
        Log.Information("Iniciada a escuta ativa de mensagens na fila_tickets.");

        while (!stoppingToken.IsCancellationRequested) 
        { 
            await Task.Delay(1000, stoppingToken); 
        }
    }
}

public record TicketReadModel(string Id, string Titulo, string Descricao, string Prioridade, string Status);