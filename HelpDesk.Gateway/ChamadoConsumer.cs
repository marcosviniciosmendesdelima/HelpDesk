using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Npgsql;
using Dapper;

public class ChamadoConsumer
{
    private readonly string _hostname = "rabbitmq"; 
    private readonly string _connectionString;

    public ChamadoConsumer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void EscutarEventos()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            UserName = "guest",
            Password = "guest"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        // Fila alinhada com o seu script Python
        const string queueName = "fila_tickets";
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        Console.WriteLine($"Gateway .NET: Aguardando mensagens na fila '{queueName}' para gravar no banco...");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($" [🚨] Chamado recebido: {message}");

            try
            {
                var ticketData = JsonSerializer.Deserialize<TicketReadModel>(message, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (ticketData != null)
                {
                    using var db = new NpgsqlConnection(_connectionString);
                    
                    // SQL corrigido usando prefixos nos parâmetros para evitar conflito com nomes de colunas
                    var sql = @"INSERT INTO ""TicketsRead"" (""Id"", ""titulo"", ""descricao"", ""prioridade"", ""status"", ""datacriacao"") 
                                VALUES (@Id, @pTitulo, @pDescricao, @pPrioridade, @pStatus, @pDataCriacao) 
                                ON CONFLICT (""Id"") DO NOTHING;";

                    // Execução com os parâmetros mapeados corretamente
                    db.Execute(sql, new {
                        Id = Guid.Parse(ticketData.Id),
                        pTitulo = ticketData.Titulo,
                        pDescricao = ticketData.Descricao,
                        pPrioridade = ticketData.Prioridade,
                        pStatus = ticketData.Status,
                        pDataCriacao = DateTime.UtcNow
                    });
                    
                    Console.WriteLine($" [✅] Ticket {ticketData.Id} gravado com sucesso no PostgreSQL!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [❌] Erro ao processar ticket: {ex.Message}");
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }
}

public record TicketReadModel(
    string Id, 
    string Titulo, 
    string Descricao, 
    string Prioridade, 
    string Status
);