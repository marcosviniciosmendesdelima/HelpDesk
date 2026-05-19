using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Services; 
using HelpDesk.Gateway.Hubs; // <-- 1. ADICIONADO: Namespace para o .NET encontrar o TicketHub 

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao Gateway (YARP) lendo o appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// REGISTRA O WORKER DO RABBITMQ
builder.Services.AddHostedService<TicketCreatedConsumer>();

// CONFIGURAÇÕES DA ETAPA 9.1 (CACHE DISTRIBUÍDO)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "helpdesk-cache:6379";
    options.InstanceName = "HelpDesk:"; 
});

builder.Services.AddScoped<ITicketCacheService, TicketCacheService>();

// CONFIGURAÇÕES DA ETAPA 9.2 (MAPEAMENTO DE CONTROLLERS)
builder.Services.AddControllers(); 

// ============================================================================
// CONFIGURAÇÕES ADICIONADAS PARA A ETAPA 10.1 (SIGNALR - TEMPO REAL)
// ============================================================================

// 2. Injeta o motor de conexões persistentes do SignalR no .NET Core
builder.Services.AddSignalR(); // <-- ADICIONADO AQUI 

// ============================================================================

var app = builder.Build();

// Mapeia as rotas dos controllers customizados locais
app.MapControllers(); 

// ============================================================================
// MAPEAMENTO DO HUB DO SIGNALR (ETAPA 10.1)
// ============================================================================

// 3. Mapeia o endpoint TCP que o frontend Angular usará para fechar a conexão WebSocket
app.MapHub<TicketHub>("/hubs/tickets"); // <-- ADICIONADO AQUI 

// ============================================================================

// Ativa o roteamento do Gateway (YARP)
app.MapReverseProxy();

app.Run();