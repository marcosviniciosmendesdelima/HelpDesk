using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using HelpDesk.Gateway.Hubs;
using Serilog;

namespace HelpDesk.Gateway.Workers
{
    public class TicketCreatedConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private readonly IHubContext<TicketHub> _hubContext;

        public TicketCreatedConsumer(IConfiguration config, IHubContext<TicketHub> hubContext)
        {
            _config = config;
            _hubContext = hubContext;
            _connectionString = _config.GetConnectionString("DefaultConnection") 
                ?? throw new Exception("Connection string não encontrada.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var rabbitHost = _config["RabbitMQ:Host"] ?? "rabbitmq";
            var factory = new ConnectionFactory() { HostName = rabbitHost, DispatchConsumersAsync = true };

            IConnection? connection = null;
            while (connection == null && !stoppingToken.IsCancellationRequested)
            {
                try { connection = factory.CreateConnection(); Log.Information("RabbitMQ conectado!"); }
                catch { await Task.Delay(5000, stoppingToken); }
            }

            using var channel = connection!.CreateModel();
            channel.QueueDeclare("fila_tickets", durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ticketData = JsonSerializer.Deserialize<TicketReadModel>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (ticketData != null)
                {
                    using var db = new NpgsqlConnection(_connectionString);
                    await db.ExecuteAsync(@"INSERT INTO ""TicketsRead"" (""Id"", ""titulo"", ""descricao"", ""prioridade"", ""status"") VALUES (@Id, @Titulo, @Descricao, @Prioridade, @Status)", 
                        new { Id = Guid.Parse(ticketData.Id), Titulo = ticketData.Titulo, Descricao = ticketData.Descricao, Prioridade = ticketData.Prioridade, Status = ticketData.Status });

                    // ENVIO SERIALIZADO (String JSON)
                    var jsonPayload = JsonSerializer.Serialize(ticketData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await _hubContext.Clients.All.SendAsync("TicketCreated", jsonPayload);
                    Log.Information("Ticket {Id} processado e enviado.", ticketData.Id);
                }
            };
            channel.BasicConsume("fila_tickets", true, consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
    public record TicketReadModel(string Id, string Titulo, string Descricao, string Prioridade, string Status);
}