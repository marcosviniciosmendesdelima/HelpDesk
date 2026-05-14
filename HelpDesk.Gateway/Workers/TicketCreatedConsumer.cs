using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HelpDesk.Gateway.Workers;

public class TicketCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public TicketCreatedConsumer(IConfiguration config)
    {
        _config = config;

        _connectionString = _config.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string não encontrada.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Conexão com RabbitMQ
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"

            // Se estiver usando Docker:
            // HostName = "rabbitmq"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Cria fila se não existir
        channel.QueueDeclare(
            queue: "fila_tickets",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Mensagem recebida: {message}");

                // Converte JSON para objeto C#
                var ticketData = JsonSerializer.Deserialize<TicketReadModel>(message);

                if (ticketData != null)
                {
                    // Conexão com banco
                    using var db = new NpgsqlConnection(_connectionString);

                    // SQL corrigido para PostgreSQL
                    var sql = @"
                        INSERT INTO ""TicketsRead""
                        (""Id"", ""Titulo"", ""Descricao"", ""Prioridade"", ""Status"")
                        VALUES
                        (@Id, @Titulo, @Descricao, @Prioridade, @Status)
                    ";

                    await db.ExecuteAsync(sql, ticketData);

                    Console.WriteLine($"[Gateway] Ticket {ticketData.Id} sincronizado!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
            }
        };

        // Escuta a fila
        channel.BasicConsume(
            queue: "fila_tickets",
            autoAck: true,
            consumer: consumer
        );

        Console.WriteLine("Ouvindo mensagens do RabbitMQ...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

// Modelo usado pelo Dapper
public record TicketReadModel(
    string Id,
    string Titulo,
    string Descricao,
    string Prioridade,
    string Status
);