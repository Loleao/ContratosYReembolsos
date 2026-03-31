using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Agency> Agencias { get; set; }
        public DbSet<Cemetery> Cementerios { get; set; }
        public DbSet<Niche> Nichos { get; set; }
        public DbSet<Pavilion> Pabellones { get; set; }
        public DbSet<Contract> Contratos { get; set; }
        public DbSet<ContractDetail> DetallesContrato { get; set; }
        public DbSet<Service> Servicios { get; set; }
        public DbSet<ServiceCategory> CategoriasServicios { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<PhysicalUnit> UnidadesFisicas { get; set; }
    }
}
