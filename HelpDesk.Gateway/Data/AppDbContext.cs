using Microsoft.EntityFrameworkCore;
using HelpDesk.Gateway.Models;

using Polly;
using Polly.Retry;

using Npgsql;
using Serilog;

using System.Net.Sockets;

namespace HelpDesk.Gateway.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ResiliencePipeline _pipeline;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,

                    Delay = TimeSpan.FromSeconds(2),

                    BackoffType = DelayBackoffType.Constant,

                    ShouldHandle = new PredicateBuilder()
                        .Handle<NpgsqlException>()
                        .Handle<SocketException>(),

                    OnRetry = args =>
                    {
                        Log.Warning(
                            "Retry executado no PostgreSQL. Tentativa: {Tentativa}",
                            args.AttemptNumber + 1);

                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public DbSet<Chamado> Chamados { get; set; }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                return await base.SaveChangesAsync(token);

            }, cancellationToken);
        }
    }
}