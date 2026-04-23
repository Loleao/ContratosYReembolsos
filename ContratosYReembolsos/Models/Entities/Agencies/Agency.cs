using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Models.Entities.Agencies
{
    public class Agency
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(11)]
        public string RUC { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; } // Útil para enviar liquidaciones automáticas

        public string Address { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }
    }
}