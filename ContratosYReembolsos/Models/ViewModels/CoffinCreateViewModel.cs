using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class CoffinCreateViewModel
    {
        [Required]
        public string ModelName { get; set; } // "Cofre Estándar"
        public string Color { get; set; }     // "Blanco"
        public string Material { get; set; }  // "Madera"
        public string Size { get; set; }      // "Adulto", "Párvulo" o "XL"
        public int InitialStock { get; set; }
        public int MinimumStock { get; set; }
    }
}
