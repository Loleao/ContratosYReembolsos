using System.ComponentModel.DataAnnotations.Schema;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Models
{
    public class BranchStock
    {
        public int Id { get; set; }

        [ForeignKey("CoffinVariant")]
        public int CoffinVariantId { get; set; }
        public CoffinVariant CoffinVariant { get; set; }

        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}
