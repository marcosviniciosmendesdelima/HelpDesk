using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Definindo a Política de Resiliência 
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Lida com erros 5xx ou falhas de rede
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); 
    // Tenta 3 vezes: espera 2s, 4s e 8s

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)); 
    // Se falhar 5 vezes seguidas, "abre o disjuntor" por 30 segundos

// 2. Aplicando no Gateway (YARP)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

app.MapReverseProxy();
app.Run();