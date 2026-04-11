using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class ContractDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        public int CoffinVariantId { get; set; }
        public virtual CoffinVariant? CoffinVariant { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Precio al que se vendió en ese momento
    }
}