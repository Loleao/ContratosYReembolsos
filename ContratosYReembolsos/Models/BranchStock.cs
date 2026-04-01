using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class BranchStock
    {
        public int Id { get; set; }

        [ForeignKey("CoffinVariant")]
        public int CoffinVariantId { get; set; }
        public CoffinVariant CoffinVariant { get; set; }

        public string SubsidiaryId { get; set; }

        public int Quantity { get; set; }
        public int MinimumStock { get; set; } // ¡Importante! Cada filial tiene su propia meta mínima
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}
