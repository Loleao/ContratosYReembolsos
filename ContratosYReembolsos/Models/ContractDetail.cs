using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class ContractDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        // Vínculo al catálogo de servicios (Ataúd, Capilla, etc.)
        public int ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        // Vínculo al Ataúd físico específico (StockItem)
        public int? StockItemId { get; set; }
        public virtual StockItem? StockItem { get; set; }

        public string Observations { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Precio al que se vendió en ese momento
    }
}