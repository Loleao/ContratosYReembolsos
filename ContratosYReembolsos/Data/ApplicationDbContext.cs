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
        public DbSet<VehicleType> TiposVehiculo { get; set; }
        public DbSet<VehicleService> VehiculosServicios { get; set; }
        public DbSet<IntermentStructureTemplate> TemplatesSepulturas { get; set; }
        public DbSet<IntermentStructure> SepulturasEstructura { get; set; }
        public DbSet<IntermentSpace> SepulturasNichos { get; set; }
        public DbSet<Branch> Filiales { get; set; }
        public DbSet<Ubigeo> Ubigeos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CoffinTransfer>()
                .HasOne(t => t.OriginBranch)
                .WithMany()
                .HasForeignKey(t => t.OriginBranchId)
                .OnDelete(DeleteBehavior.Restrict); // Desactivar cascada en el origen

            modelBuilder.Entity<CoffinTransfer>()
                .HasOne(t => t.TargetBranch)
                .WithMany()
                .HasForeignKey(t => t.TargetBranchId)
                .OnDelete(DeleteBehavior.NoAction); // Desactivar cascada en el destino

            // También es recomendable hacerlo para los movimientos si dan error similar
            modelBuilder.Entity<CoffinMovement>()
                .HasOne(m => m.Branch)
                .WithMany()
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Branch)
                .WithMany() // O .WithMany(b => b.Contracts) si tienes la colección
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict); // <--- CAMBIO AQUÍ

            // Evitar cascada múltiple en Contratos -> Agencias
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Agency)
                .WithMany()
                .HasForeignKey(c => c.AgencyId)
                .OnDelete(DeleteBehavior.Restrict); // <--- CAMBIO AQUÍ

            modelBuilder.Entity<VehicleService>() // Ajusta al nombre de tu clase C#
                .HasOne(vs => vs.Vehicle)
                .WithMany()
                .HasForeignKey(vs => vs.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Relación con Conductores (No cascada)
            modelBuilder.Entity<VehicleService>()
                .HasOne(vs => vs.Driver)
                .WithMany()
                .HasForeignKey(vs => vs.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relación con Contratos (No cascada)
            modelBuilder.Entity<VehicleService>()
                .HasOne(vs => vs.Contract)
                .WithMany()
                .HasForeignKey(vs => vs.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
