using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers(); // GARANTA ESTA LINHA AQUI

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHostedService<TicketCreatedConsumer>();

var app = builder.Build();

app.UseCors();
app.UseWebSockets();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TicketsHub>("/hubs/tickets");
    endpoints.MapControllers(); // E GARANTA ESTA LINHA AQUI
    endpoints.MapReverseProxy();
});

app.Run();