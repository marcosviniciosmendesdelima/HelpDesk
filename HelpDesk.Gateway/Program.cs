using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Services;
using HelpDesk.Gateway.Hubs;

using Serilog;
using Serilog.Events;

using StackExchange.Redis;

// ============================================================================
// CONFIGURAÇÃO DO SERILOG
// ============================================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Inicializando HelpDesk.Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    // =========================================================================
    // ATIVA O SERILOG
    // =========================================================================

    builder.Host.UseSerilog();

    // =========================================================================
    // CONFIGURAÇÃO DINÂMICA REDIS
    // =========================================================================

    string? redisHostConfig = builder.Configuration["Redis:Host"];

    string redisHost = !string.IsNullOrEmpty(redisHostConfig)
        ? redisHostConfig
        : "localhost";

    string redisConnection = $"{redisHost}:6379";

    Log.Information("Redis configurado para {RedisHost}", redisConnection);

    // =========================================================================
    // CONFIGURAÇÃO DO YARP (API GATEWAY)
    // =========================================================================

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // =========================================================================
    // REGISTRA O WORKER DO RABBITMQ
    // =========================================================================

    builder.Services.AddHostedService<TicketCreatedConsumer>();

    // =========================================================================
    // CACHE DISTRIBUÍDO REDIS
    // =========================================================================

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "HelpDesk:";
    });

    builder.Services.AddScoped<ITicketCacheService, TicketCacheService>();

    // =========================================================================
    // CONTROLLERS
    // =========================================================================

    builder.Services.AddControllers();

    // =========================================================================
    // CORS
    // =========================================================================

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed(_ => true);
        });
    });

    // =========================================================================
    // SIGNALR + REDIS BACKPLANE
    // =========================================================================

    builder.Services.AddSignalR()
        .AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix =
                RedisChannel.Literal("HelpDesk_SignalR");
        });

    // =========================================================================

    var app = builder.Build();

    // =========================================================================
    // MIDDLEWARE DO CORRELATION ID
    // =========================================================================

    app.Use(async (context, next) =>
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next();
        }
    });

    // =========================================================================

    app.UseCors();

    app.UseWebSockets();

    app.UseRouting();

    // =========================================================================
    // MAPEAMENTO DOS ENDPOINTS
    // =========================================================================

    app.MapControllers();

    app.MapHub<TicketHub>("/hubs/tickets");

    app.MapReverseProxy();

    // =========================================================================
    // LOG DE INICIALIZAÇÃO
    // =========================================================================

    Log.Information("HelpDesk.Gateway inicializado com sucesso.");

    // =========================================================================
    // EXECUÇÃO DA APLICAÇÃO
    // =========================================================================

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O Gateway encerrou inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}