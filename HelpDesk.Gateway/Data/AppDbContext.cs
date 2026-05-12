using Microsoft.EntityFrameworkCore;
using HelpDesk.Gateway.Models;

namespace HelpDesk.Gateway.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Chamado> Chamados { get; set; }
    }
}