using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Coffin
    {
        public int Id { get; set; }

        public string ModelName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<CoffinVariant> Variants { get; set; }
    }
}