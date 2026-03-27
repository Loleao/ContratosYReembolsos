using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Niche
    {
        [Key]
        public int Id { get; set; }
        public string Zone { get; set; }
        public int Row { get; set; }  
        public int Number { get; set; } 
        public bool IsOccupied { get; set; } 
        public string CemeteryId { get; set; }
    }
}
