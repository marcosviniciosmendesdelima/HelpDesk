using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DE OBSERVABILIDADE (SERILOG) ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// --- POLÍTICAS DE RESILIÊNCIA (POLLY) ---

// Política de Retry: Tenta 3 vezes antes de desistir
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        3,
        _ => TimeSpan.FromSeconds(2),
        (result, timeSpan, retryCount, context) =>
        {
            Log.Warning($"[RETRY] Tentativa {retryCount} falhou. Tentando novamente em {timeSpan.TotalSeconds}s...");
        });

// --- [PARTE DO LUIS] CIRCUIT BREAKER (DISJUNTOR) ---
// Se falhar 3 vezes seguidas, o circuito "abre" por 15 segundos
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(15),
        onBreak: (result, timespan) =>
        {
            Log.Error($"--- CIRCUITO ABERTO: Sistema em quarentena por {timespan.TotalSeconds}s devido a falhas consecutivas ---");
        },
        onReset: () =>
        {
            Log.Information("--- CIRCUITO FECHADO: Conectividade restabelecida com a API! ---");
        });

// --- CONFIGURAÇÃO DO GATEWAY (YARP) ---
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Vincula as políticas de resiliência ao cliente HTTP que o Gateway usa
builder.Services.AddHttpClient("GatewayClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

// Ativa o roteamento do Gateway
app.MapReverseProxy();

// --- EXECUÇÃO COM MONITORAMENTO DE CICLO DE VIDA ---
try
{
    Log.Information("Iniciando Gateway HelpDesk com Força Máxima...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O Gateway falhou ao iniciar inesperadamente!");
}
finally
{
    Log.CloseAndFlush();
}