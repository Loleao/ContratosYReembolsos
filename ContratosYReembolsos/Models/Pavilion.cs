using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Pavilion
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string CemeteryId { get; set; }
    }
}
