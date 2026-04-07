//using Microsoft.EntityFrameworkCore;
//using ContratosYReembolsos.Models;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

//namespace ContratosYReembolsos.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

//        public DbSet<Agency> Agencias { get; set; }
//        public DbSet<Cemetery> Cementerios { get; set; }
//        public DbSet<Niche> Nichos { get; set; }
//        public DbSet<Pavilion> Pabellones { get; set; }
//        public DbSet<Contract> Contratos { get; set; }
//        public DbSet<ContractDetail> DetallesContrato { get; set; }
//        public DbSet<ContractMovilityDetail> DetallesMovilidadContrato { get; set; }
//        public DbSet<Service> Servicios { get; set; }
//        public DbSet<ServiceCategory> CategoriasServicios { get; set; }
//        public DbSet<StockItem> StockItems { get; set; }
//        public DbSet<PhysicalUnit> UnidadesFisicas { get; set; }
//        public DbSet<Coffin> Ataudes { get; set; }
//        public DbSet<CoffinMovement> MovimientosAtaudes { get; set; }
//        public DbSet<BranchStock> StockFilial { get; set; }
//        public DbSet<CoffinVariant> AtaudVariantes { get; set; }
//        public DbSet<CoffinTransfer> AtaudTransferencias { get; set; }
//        public DbSet<Driver> Conductores { get; set; }
//        public DbSet<Vehicle> Vehiculos { get; set; }
//        public DbSet<VehicleService> VehiculosServicios { get; set; }
//        public DbSet<IntermentStructureTemplate> TemplatesSepulturas { get; set; }
//        public DbSet<IntermentStructure> SepulturasEstructura { get; set; }
//        public DbSet<IntermentSpace> SepulturasNichos { get; set; }
//        public DbSet<Branch> Filiales { get; set; }
//        public DbSet<Ubigeo> Ubigeos { get; set; }
//    }
//}

using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.AspNetCore.Identity; 

namespace ContratosYReembolsos.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Agency> Agencias { get; set; }
        public DbSet<Cemetery> Cementerios { get; set; }
        public DbSet<Niche> Nichos { get; set; }
        public DbSet<Pavilion> Pabellones { get; set; }
        public DbSet<Contract> Contratos { get; set; }
        public DbSet<ContractDetail> DetallesContrato { get; set; }
        public DbSet<ContractMovilityDetail> DetallesMovilidadContrato { get; set; }
        public DbSet<Service> Servicios { get; set; }
        public DbSet<ServiceCategory> CategoriasServicios { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<PhysicalUnit> UnidadesFisicas { get; set; }
        public DbSet<Coffin> Ataudes { get; set; }
        public DbSet<CoffinMovement> MovimientosAtaudes { get; set; }
        public DbSet<BranchStock> StockFilial { get; set; }
        public DbSet<CoffinVariant> AtaudVariantes { get; set; }
        public DbSet<CoffinTransfer> AtaudTransferencias { get; set; }
        public DbSet<Driver> Conductores { get; set; }
        public DbSet<Vehicle> Vehiculos { get; set; }
        public DbSet<VehicleService> VehiculosServicios { get; set; }
        public DbSet<IntermentStructureTemplate> TemplatesSepulturas { get; set; }
        public DbSet<IntermentStructure> SepulturasEstructura { get; set; }
        public DbSet<IntermentSpace> SepulturasNichos { get; set; }
        public DbSet<Branch> Filiales { get; set; }
        public DbSet<Ubigeo> Ubigeos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
