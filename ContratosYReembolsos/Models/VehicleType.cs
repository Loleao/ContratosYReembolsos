using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class VehicleType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Icon { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
