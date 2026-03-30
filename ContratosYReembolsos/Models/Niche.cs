using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class Niche
    {
        [Key]
        public int Id { get; set; }

        public int PavilionId { get; set; }
        [ForeignKey("PavilionId")]
        public Pavilion Pavilion { get; set; }

        public string Row { get; set; }  
        public int Column { get; set; } 
        public bool IsOccupied { get; set; } 
        public string CemeteryId { get; set; }
        public bool IsBeingReserved { get; set; } // El cuadro gris
        public DateTime? ReservationExpiry { get; set; } // Cuándo vence la reserva temporal
        public string? ReservedByToken { get; set; } // Quién lo tiene bloqueado (ID de sesión)
    }
}
