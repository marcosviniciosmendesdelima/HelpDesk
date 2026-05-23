using System.Text;
using System.Text.Json;

using Dapper;
using Npgsql;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

using HelpDesk.Gateway.Hubs;
using HelpDesk.Gateway.Services;

using Serilog;

namespace HelpDesk.Gateway.Workers
{
    public class TicketCreatedConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString;

        public TicketCreatedConsumer(
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;

            _connectionString = _config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Connection string não encontrada.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string? configHost = _config["RabbitMQ:Host"];

            string rabbitHost = !string.IsNullOrEmpty(configHost)
                ? configHost
                : "localhost";

            if (rabbitHost == "helpdesk-rabbitmq" &&
                !Environment.GetEnvironmentVariables()
                    .Contains("DOTNET_RUNNING_IN_CONTAINER"))
            {
                rabbitHost = "localhost";
            }

            var factory = new ConnectionFactory()
            {
                HostName = rabbitHost,
                DispatchConsumersAsync = true
            };

            IConnection? connection = null;
            int tentativas = 0;

            Log.Information("Tentando conectar ao RabbitMQ em {Host}", rabbitHost);

            while (connection == null &&
                   !stoppingToken.IsCancellationRequested &&
                   tentativas < 10)
            {
                try
                {
                    tentativas++;

                    connection = factory.CreateConnection();

                    Log.Information("RabbitMQ conectado com sucesso");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "RabbitMQ indisponível. Tentando novamente em 5s...");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            if (connection == null)
            {
                Log.Fatal("Falha ao conectar no RabbitMQ após múltiplas tentativas.");
                return;
            }

            using var currentConnection = connection;
            using var channel = currentConnection.CreateModel();

            const string queueName = "fila_tickets";

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                    Log.Information("Mensagem recebida: {Message}", message);

                    var ticketData = JsonSerializer.Deserialize<TicketReadModel>(
                        message,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (ticketData == null)
                    {
                        Log.Warning("Mensagem inválida recebida");
                        return;
                    }

                    // ============================================================
                    // POSTGRES
                    // ============================================================

                    try
                    {
                        using var db = new NpgsqlConnection(_connectionString);

                        var sql = @"
                            INSERT INTO ""TicketsRead""
                            (
                                ""Id"",
                                ""Titulo"",
                                ""Descricao"",
                                ""Prioridade"",
                                ""Status""
                            )
                            VALUES
                            (
                                @Id,
                                @Titulo,
                                @Descricao,
                                @Prioridade,
                                @Status
                            )
                            ON CONFLICT (""Id"") DO NOTHING;
                        ";

                        await db.ExecuteAsync(sql, ticketData);

                        Log.Information("Ticket salvo no PostgreSQL: {Id}", ticketData.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Erro ao salvar no PostgreSQL");
                    }

                    // ============================================================
                    // CACHE REDIS (CORRIGIDO - SCOPED SAFE)
                    // ============================================================

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var cacheService =
                            scope.ServiceProvider.GetRequiredService<ITicketCacheService>();

                        await cacheService.RemoveTicketCacheAsync("tickets:all");
                    }

                    Log.Information("Cache invalidado");

                    // ============================================================
                    // SIGNALR
                    // ============================================================

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var hubContext =
                            scope.ServiceProvider.GetRequiredService<IHubContext<TicketHub>>();

                        Guid ticketGuid = Guid.Parse(ticketData.Id);

                        string grupo = TicketHub.ObterNomeDoGrupo(ticketGuid);

                        await hubContext.Clients
                            .Group(grupo)
                            .SendAsync("OnTicketStatusChanged", new
                            {
                                ticketId = ticketData.Id,
                                titulo = ticketData.Titulo,
                                status = ticketData.Status,
                                prioridade = ticketData.Prioridade,
                                notificadoEm = DateTime.UtcNow,
                                mensagem = "Ticket atualizado em tempo real"
                            });

                        Log.Information("SignalR enviado para grupo {Grupo}", grupo);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro crítico no consumer");
                }
            };

            channel.BasicConsume(queueName, autoAck: true, consumer);

            Log.Information("Consumer iniciado na fila {Queue}", queueName);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    public record TicketReadModel(
        string Id,
        string Titulo,
        string Descricao,
        string Prioridade,
        string Status);
}