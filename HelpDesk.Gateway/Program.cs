using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Services;
using HelpDesk.Gateway.Hubs;

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
// CONFIGURAÇÕES ADICIONADAS PARA A ETAPA 10.1 E 10.2
// (SIGNALR + REDIS BACKPLANE)
// ============================================================================

// Injeta o SignalR e configura o Redis como Backplane distribuído
builder.Services.AddSignalR()
    .AddStackExchangeRedis("helpdesk-cache:6379", options =>
    {
        options.Configuration.ChannelPrefix = "HelpDesk_SignalR";
    });

// ============================================================================

var app = builder.Build();

// Mapeia as rotas dos controllers customizados locais
app.MapControllers();

// ============================================================================
// MAPEAMENTO DO HUB DO SIGNALR (ETAPA 10.1)
// ============================================================================

// Endpoint WebSocket usado pelo frontend
app.MapHub<TicketHub>("/hubs/tickets");

// ============================================================================

// Ativa o roteamento do Gateway (YARP)
app.MapReverseProxy();

app.Run();