using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE OBSERVABILIDADE (SERILOG) ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-audit.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. POLÍTICAS DE RESILIÊNCIA (POLLY) ---
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        3,
        _ => TimeSpan.FromSeconds(2),
        (result, timeSpan, retryCount, context) =>
        {
            Log.Warning($"[RETRY] Tentativa {retryCount} falhou.");
        });

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(15)
    );

// --- 3. CONFIGURAÇÃO DO GATEWAY (YARP) ---
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// --- 4. HTTP CLIENTS ---
builder.Services.AddHttpClient("CatalogoServiceClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/");
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient("GatewayClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

app.MapReverseProxy();


// ===============================
// RABBITMQ CONSUMER
// ===============================
var consumer = new ChamadoConsumer();

_ = Task.Run(async () =>
{
    await consumer.EscutarEventos();
});


// --- EXECUÇÃO ---
try
{
    Log.Information("Gateway iniciado.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro ao iniciar gateway.");
}
finally
{
    Log.CloseAndFlush();
}