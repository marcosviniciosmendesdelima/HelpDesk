using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class ChamadoConsumer
{
    // Ajustado para o nome do serviço no Docker
    private readonly string _hostname = "rabbitmq"; 
    private readonly string _exchangeName = "chamado.criado";

    public void EscutarEventos()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            UserName = "guest",
            Password = "guest"
        };

        // Versão síncrona para compatibilidade com o Build atual
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Fanout
        );

        // Declara uma fila temporária
        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(
            queue: queueName,
            exchange: _exchangeName,
            routingKey: ""
        );

        Console.WriteLine("Gateway .NET: Aguardando mensagens do Python...");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [🚨 GATEWAY .NET] Chamado recebido do Python: {message}");
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: true,
            consumer: consumer
        );
    }
}