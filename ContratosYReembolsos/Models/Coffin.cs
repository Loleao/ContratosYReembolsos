using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Coffin
    {
        public int Id { get; set; }

        public string ModelName { get; set; } // Ej: Imperial, Presidencial, Económico

        public string Material { get; set; } // Madera, Metal, Cobre

        public string Color { get; set; }

        public string Size { get; set; } // Adulto, Párvulo, Extra Grande

        [Range(0, 9999)]
        public int CurrentStock { get; set; }

        public int MinimumStock { get; set; } // Para alertas en el Dashboard

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public string ImageUrl { get; set; } // Ruta de la foto en wwwroot/images/ataudes

        public bool IsActive { get; set; } = true;
    }
}
