using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR; 
using HelpDesk.Gateway.Hubs;       
using Microsoft.Extensions.DependencyInjection; 

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
        var factory = new ConnectionFactory() { HostName = "helpdesk-rabbitmq" };
        IConnection connection = null;
        int tentativas = 0;

        while (connection == null && !stoppingToken.IsCancellationRequested && tentativas < 10)
        {
            try
            {
                tentativas++;
                connection = factory.CreateConnection();
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
            {
                Console.WriteLine($"[Gateway] Aguardando RabbitMQ... Tentativa {tentativas}/10");
                await Task.Delay(5000, stoppingToken); 
            }
        }

        using var currentConnection = connection;
        using var channel = currentConnection.CreateModel();

        channel.QueueDeclare(queue: "fila_tickets", durable: true, exclusive: false, autoDelete: false);
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem recebida: {message}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var ticketData = JsonSerializer.Deserialize<TicketReadModel>(message, options);

                if (ticketData != null)
                {
                    Console.WriteLine($"[Gateway] Objeto convertido! Id: {ticketData.Id}, Status: {ticketData.Status}");

                    // DISPARO SIGNALR VIA ESCOPO (FUNCIONANDO!)
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TicketsHub>>();
                        
                        var payloadNotificacao = new Dictionary<string, string>
                        {
                            { "ticketId", ticketData.Id ?? "Sem ID" },
                            { "status", ticketData.Status ?? "Sem Status" }
                        };

                        await hubContext.Clients.All.SendAsync("OnTicketStatusChanged", payloadNotificacao);
                        Console.WriteLine($"[Gateway] Notificação enviada via WebSocket para o Navegador!");
                    }

                    // Grava no banco
                    try
                    {
                        using var db = new NpgsqlConnection(_connectionString);
                        var sql = @"INSERT INTO ""TicketsRead"" (""Id"", ""Titulo"", ""Descricao"", ""Prioridade"", ""Status"") 
                                    VALUES (@Id, @Titulo, @Descricao, @Prioridade, @Status)";
                        await db.ExecuteAsync(sql, ticketData);
                        Console.WriteLine($"[Gateway] Ticket {ticketData.Id} sincronizado no Banco!");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"[Gateway] Erro no banco (Ignorado para o Real-time): {dbEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro crítico ao processar mensagem: {ex.Message}");
            }
        };

        channel.BasicConsume(queue: "fila_tickets", autoAck: true, consumer: consumer);
        Console.WriteLine("Ouvindo mensagens do RabbitMQ...");

        while (!stoppingToken.IsCancellationRequested) { await Task.Delay(1000, stoppingToken); }
    }
}

public record TicketReadModel(string Id, string Titulo, string Descricao, string Prioridade, string Status);