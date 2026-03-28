using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using ContratosYReembolsos.Models.External;

namespace ContratosYReembolsos.Data
{
    public class LimaContractsDbContext : DbContext
    {
        public LimaContractsDbContext(DbContextOptions<LimaContractsDbContext> options) : base(options) { }

        public DbSet<Affiliate> Afiliados { get; set; }
        public DbSet<Beneficiary> Beneficiarios { get; set; }
        public DbSet<Subsidiary> Filiales { get; set; }
        public DbSet<Ubigeo> Ubigeos { get; set; }
        public DbSet<Wake> Velatorios { get; set; }
    }
}
