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

            // --- CONFIGURACIÓN DE TRASLADOS Y MOVIMIENTOS ---
            modelBuilder.Entity<CoffinTransfer>()
                .HasOne(t => t.OriginBranch).WithMany().HasForeignKey(t => t.OriginBranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoffinTransfer>()
                .HasOne(t => t.TargetBranch).WithMany().HasForeignKey(t => t.TargetBranchId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CoffinMovement>()
                .HasOne(m => m.Branch).WithMany().HasForeignKey(m => m.BranchId).OnDelete(DeleteBehavior.Restrict);

            // --- CONFIGURACIÓN DE CONTRATOS (RELACIONES PRINCIPALES) ---
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Branch).WithMany().HasForeignKey(c => c.BranchId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Agency).WithMany().HasForeignKey(c => c.AgencyId).OnDelete(DeleteBehavior.Restrict);

            // Relación con Ubigeo del fallecido
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Ubigeo).WithMany().HasForeignKey(c => c.UbigeoId).OnDelete(DeleteBehavior.Restrict);

            // --- LOGÍSTICA Y MOVILIDAD ---
            modelBuilder.Entity<ContractMovilityDetail>()
                .HasOne(d => d.Contract).WithMany(c => c.MovilityDetails).HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleService>()
                .HasOne(vs => vs.Vehicle).WithMany().HasForeignKey(vs => vs.VehicleId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleService>()
                .HasOne(vs => vs.Driver).WithMany().HasForeignKey(vs => vs.DriverId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleService>()
                .HasOne(vs => vs.Contract).WithMany().HasForeignKey(vs => vs.ContractId).OnDelete(DeleteBehavior.Restrict);

            // --- INFRAESTRUCTURA (FILIALES, CEMENTERIOS, NICHOS) ---
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Ubigeo).WithMany().HasForeignKey(b => b.UbigeoId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cemetery>()
                .HasOne(c => c.Ubigeo).WithMany().HasForeignKey(c => c.UbigeoId).OnDelete(DeleteBehavior.Restrict);

            // Relación Nicho -> Estructura (Evita el error de cascada en la creación de la tabla nichos)
            modelBuilder.Entity<IntermentSpace>()
                .HasOne(n => n.Structure).WithMany(e => e.Spaces).HasForeignKey(n => n.StructureId).OnDelete(DeleteBehavior.Restrict);

            // --- LA CLAVE PARA ELIMINAR CONTRACTID1 ---
            // Configuramos la relación 1 a 1 de forma explícita.
            // Contract es el dueño de la relación (tiene IntermentSpaceId).
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.IntermentSpace)
                .WithMany() // No ponemos .WithOne(n => n.Contract) aquí si no es necesario
                .HasForeignKey(c => c.IntermentSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Si tu clase IntermentSpace tiene un campo "ContractId" para saber quién lo ocupa:
            modelBuilder.Entity<IntermentSpace>()
                .HasOne<Contract>()
                .WithMany() // O nada si no hay colección de nichos en Contract
                .HasForeignKey(n => n.ContractId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
