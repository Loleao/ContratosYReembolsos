using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Cemetery
    {
        [Key]
        public int Id { get; set; }

        public string RUC { get; set; }

        public string Name { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<IntermentStructure>? Structures { get; set; }
    }
}
