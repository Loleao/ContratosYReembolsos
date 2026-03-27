using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Agency> Agencias { get; set; }
        public DbSet<Cemetery> Cementerios { get; set; }
        public DbSet<ServiceContract> Contracts { get; set; }
    }
}
