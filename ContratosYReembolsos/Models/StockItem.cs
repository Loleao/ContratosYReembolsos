using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class StockItem
    {
        [Key]
        public int Id { get; set; }

        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }

        public string SerialNumber { get; set; } // El código único del ataúd
        public string Status { get; set; } // "Disponible", "Vendido", "Reservado"
    }
}
