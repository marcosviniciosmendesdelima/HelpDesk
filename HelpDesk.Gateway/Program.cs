using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Configura os logs pra gente ver o que rola no console e no arquivo
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-audit.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Se a rede der soluço, tenta 3 vezes antes de desistir
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2));

// Se o outro serviço cair feio, o "disjuntor" abre por 15 segundos pra não travar tudo
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpClient("GatewayClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

app.MapReverseProxy();

// Liga o carinha que fica ouvindo as mensagens do RabbitMQ
var consumer = new ChamadoConsumer();

Task.Run(() =>
{
    try
    {
        Log.Information("Ouvindo as mensagens que chegam do RabbitMQ...");
        consumer.EscutarEventos();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Deu ruim ao tentar ouvir o RabbitMQ.");
    }
});

// Coloca o Gateway pra rodar de fato
try
{
    Log.Information("Tudo pronto! Gateway no ar.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O Gateway não conseguiu subir.");
}
finally
{
    Log.CloseAndFlush();
}