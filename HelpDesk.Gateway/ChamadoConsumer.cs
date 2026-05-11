using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class ChamadoConsumer
{
    private readonly string _hostname = "localhost";
    private readonly string _exchangeName = "chamado.criado";

    public async Task EscutarEventos()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            UserName = "guest",
            Password = "guest"
        };

        var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Fanout
        );

        var queue = await channel.QueueDeclareAsync();

        var queueName = queue.QueueName;

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: _exchangeName,
            routingKey: ""
        );

        Console.WriteLine("Aguardando mensagens do Python...");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Chamado recebido: {message}");

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite);
    }
}