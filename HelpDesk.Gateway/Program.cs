using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Services; // 1. Adicionado para o .NET reconhecer a pasta de serviços

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao Gateway (YARP) lendo o appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// REGISTRA O WORKER DO RABBITMQ (Corrigido para o nome correto da classe da Etapa 8)
builder.Services.AddHostedService<TicketCreatedConsumer>();

// ============================================================================
// CONFIGURAÇÕES ADICIONADAS PARA A ETAPA 9.1 (CACHE DISTRIBUÍDO)
// ============================================================================

// 2. Configura a conexão com o container Redis que declaramos no docker-compose
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "helpdesk-cache:6379";
    options.InstanceName = "HelpDesk:"; // Prefixo para organizar as chaves no Redis
});

// 3. Registra o serviço customizado de cache na Injeção de Dependência do .NET
builder.Services.AddScoped<ITicketCacheService, TicketCacheService>();

// ============================================================================

var app = builder.Build();

// Ativa o roteamento do Gateway
app.MapReverseProxy();

app.Run();