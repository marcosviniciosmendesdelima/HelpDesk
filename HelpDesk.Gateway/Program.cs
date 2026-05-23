using HelpDesk.Gateway.Workers;
using HelpDesk.Gateway.Hubs;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para a Etapa 11.1
// Aqui estamos definindo como o sistema vai registrar tudo o que acontece.
Log.Logger = new LoggerConfiguration()
    // Define que o sistema vai capturar logs desde o nível de depuração (Debug)
    .MinimumLevel.Debug()
    // Evita que mensagens internas da Microsoft fiquem poluindo a tela, mostrando apenas o que for relevante
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    // Permite adicionar informações extras ao contexto de cada log de forma dinâmica
    .Enrich.FromLogContext()
    // Coloca automaticamente o identificador único de rastreamento em cada linha gerada
    .Enrich.WithCorrelationId() 
    // Transforma a saída do texto em formato JSON estruturado, facilitando a leitura por ferramentas do Docker
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()) 
    .CreateLogger();

// Avisa o servidor web para substituir o sistema de logs padrão pelo Serilog que configuramos acima
builder.Host.UseSerilog();

// Registrando as dependências e serviços essenciais do seu Gateway original
builder.Services.AddSignalR();
builder.Services.AddControllers();

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

// Middleware do Correlation ID para a Etapa 11.1
// Esse bloco funciona como uma guarita: toda requisição que entra na API ganha um crachá único de identificação.
app.Use(async (context, next) =>
{
    // Verifica se quem chamou a API já enviou um ID no cabeçalho. Se não enviou, nós geramos um ID novo e aleatório.
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    
    // Devolvemos o mesmo ID na resposta para que o cliente saiba qual código representou a sua chamada
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    // Vincula esse ID ao Serilog para que todas as linhas de log geradas nesta requisição fiquem associadas a ele
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        // Passa a requisição para o próximo passo do sistema prosseguir com o atendimento
        await next();
    }
});

app.UseCors();
app.UseWebSockets();
app.UseRouting();

// Mapeamento direto de rotas de nível superior (padrão moderno do .NET 8 que remove o aviso ASP0014)
app.MapHub<TicketsHub>("/hubs/tickets");
app.MapControllers();
app.MapReverseProxy();

// Registrando a inicialização do Gateway como um evento informativo importante no sistema
Log.Information("HelpDesk.Gateway inicializado com sucesso. Logs estruturados ativos.");

// Proteção do ciclo de vida da aplicação usando a estrutura Try/Catch/Finally
try
{
    // Registra que o servidor está prestes a começar a escutar as requisições
    Log.Information("Iniciando o Host da API Gateway...");
    app.Run();
}
catch (Exception ex)
{
    // Se o sistema cair por uma falha grave inesperada, o erro exato será registrado com prioridade máxima
    Log.Fatal(ex, "O Host da API Gateway terminou inesperadamente.");
}
finally
{
    // Garante que, mesmo em caso de erro crítico, todos os logs na memória sejam salvos no console antes de fechar tudo
    Log.CloseAndFlush(); 
}