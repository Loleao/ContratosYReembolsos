using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class CoffinVariant
    {
        public int Id { get; set; }

        [ForeignKey("Coffin")]
        public int CoffinModelId { get; set; }
        public virtual Coffin Coffin { get; set; }

        public string Color { get; set; } //
        public string Material { get; set; } // "Madera Caoba"
        public string Size { get; set; } // "A"Blanco", "Nogal", "Plata"
        public string? ImageUrl { get; set; } // La foto específica de ese color

        public virtual ICollection<BranchStock> Stocks { get; set; }
    }
}
