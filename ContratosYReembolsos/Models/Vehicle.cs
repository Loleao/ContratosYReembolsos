using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Plate { get; set; } // Placa (Ej: ABC-123)

        public string Model { get; set; } // Ej: Mercedes-Benz Sprinter

        public string Brand { get; set; } // Marca

        public int Year { get; set; }

        [Required]
        public string SubsidiaryId { get; set; }

        public string CurrentStatus { get; set; } = "DISPONIBLE";

        public bool IsActive { get; set; } = true;
    }
}
