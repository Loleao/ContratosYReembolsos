using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la filial es obligatorio")]
        public string Name { get; set; }

        [Required]
        public string UbigeoId { get; set; }
        [ForeignKey("UbigeoId")]
        public virtual Ubigeo Ubigeo { get; set; }

        public string Code { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasWakeService { get; set; } 
        public bool HasOwnCemetery { get; set; }

        public virtual ICollection<Cemetery> Cemeteries { get; set; } = new List<Cemetery>();
        public virtual ICollection<Agency> Agencies { get; set; } = new List<Agency>();
    }
}