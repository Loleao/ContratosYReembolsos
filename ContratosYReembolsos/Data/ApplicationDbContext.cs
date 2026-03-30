using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Agency> Agencias { get; set; }
        public DbSet<Cemetery> Cementerios { get; set; }
        public DbSet<ServiceContract> Contratos { get; set; }
        public DbSet<Niche> Nichos { get; set; }
        public DbSet<Pavilion> Pabellones { get; set; }
    }
}
