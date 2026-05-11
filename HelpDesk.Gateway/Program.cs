using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE OBSERVABILIDADE (SERILOG) ---
// Registra logs em console e arquivo para monitorar falhas e latência.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-audit.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. POLÍTICAS DE RESILIÊNCIA (POLLY) ---
// Tratamento de Falhas.

// Política de Retry (Retentativa): Tenta 3 vezes antes de desistir.
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        3, 
        _ => TimeSpan.FromSeconds(2),
        (result, timeSpan, retryCount, context) =>
        {
            Log.Warning($"[RETRY] Tentativa {retryCount} falhou. Tentando novamente em {timeSpan.TotalSeconds}s...");
        });

// Política de Circuit Breaker (Disjuntor): Abre se houver 3 falhas seguidas.
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
            Log.Information("--- CIRCUITO FECHADO: Conectividade restabelecida! ---");
        });

// --- 3. CONFIGURAÇÃO DO GATEWAY (YARP) ---
// Ponto de entrada único via Roteamento.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// --- 4. CONFIGURAÇÃO DA COMUNICAÇÃO ENTRE SERVIÇOS ---
// Aqui configuramos o IHttpClientFactory para o Service A falar com o Service B (Victor).
builder.Services.AddHttpClient("CatalogoServiceClient", client =>
{
    // O endereço onde o Microsserviço de Catálogo do Victor está rodando.
    client.BaseAddress = new Uri("http://localhost:5001/"); 
})
.AddPolicyHandler(retryPolicy)           // Aplica Retry na comunicação interna
.AddPolicyHandler(circuitBreakerPolicy); // Aplica Circuit Breaker na comunicação interna

// Vincula também as políticas ao cliente padrão do Gateway
builder.Services.AddHttpClient("GatewayClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

// Ativa o roteamento do Gateway
app.MapReverseProxy();

// --- 5. EXECUÇÃO E CICLO DE VIDA ---
try
{
    Log.Information("--- Gateway HelpDesk: Etapa 06 - Roteamento, Resiliência e Integração Ativos ---");
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