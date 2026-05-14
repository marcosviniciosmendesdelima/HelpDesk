using HelpDesk.Gateway.Workers;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao Gateway (YARP) lendo o appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// REGISTRA O WORKER DO RABBITMQ
builder.Services.AddHostedService<TicketCreatedConsumer>();

var app = builder.Build();

// Ativa o roteamento do Gateway
app.MapReverseProxy();

app.Run();