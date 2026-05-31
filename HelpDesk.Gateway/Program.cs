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

    builder.Host.UseSerilog();

    string? redisHostConfig = builder.Configuration["Redis:Host"];
    string redisHost = !string.IsNullOrEmpty(redisHostConfig) ? redisHostConfig : "localhost";
    string redisConnection = $"{redisHost}:6379";

    Log.Information("Redis configurado para {RedisHost}", redisConnection);

    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddHostedService<TicketCreatedConsumer>();

    // Mantido para o Cache, mas removido do SignalR abaixo
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "HelpDesk:";
    });

    builder.Services.AddScoped<ITicketCacheService, TicketCacheService>();

    builder.Services.AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowed(_ => true)
                  .AllowCredentials();
        });
    });

    // =========================================================================
    // SIGNALR SEM REDIS BACKPLANE (Modo Memória)
    // =========================================================================
    builder.Services.AddSignalR(); 

    var app = builder.Build();

    app.UseCors();
    app.UseRouting();
    app.UseWebSockets();

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

    app.MapControllers();
    app.MapHub<TicketHub>("/hubs/tickets");
    app.MapReverseProxy();

    Log.Information("HelpDesk.Gateway inicializado com sucesso (SignalR em Memória).");

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