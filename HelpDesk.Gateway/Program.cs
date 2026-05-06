using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DE OBSERVABILIDADE (SERILOG) ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Exibe logs coloridos no terminal do seu Mac
    .WriteTo.File("logs/gateway-log.txt", rollingInterval: RollingInterval.Day) // Grava a "Caixa-Preta"
    .CreateLogger();

builder.Host.UseSerilog();

// --- POLÍTICAS DE RESILIÊNCIA (POLLY) ---
// Política de Retry: Tenta 3 vezes antes de desistir
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
    {
        Log.Warning($"[RETRY] Tentativa {retryCount} falhou. Tentando novamente em {timeSpan.TotalSeconds}s...");
    });

// Política de Circuit Breaker: Abre o disjuntor após 5 falhas consecutivas
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
        onBreak: (result, timespan) => Log.Error("--- CIRCUITO ABERTO! Gateway isolado por 30s para evitar sobrecarga ---"),
        onReset: () => Log.Information("--- Circuito fechado. Operação normal restabelecida ---"));

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
    app.Run(); // Este comando mantém o servidor rodando e ouvindo requisições
}
catch (Exception ex) 
{
    Log.Fatal(ex, "O Gateway falhou ao iniciar inesperadamente!");
}
finally 
{
    Log.CloseAndFlush(); // Garante que todos os logs sejam gravados antes de fechar
}