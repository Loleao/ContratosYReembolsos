using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        public string Category { get; set; } 

        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
