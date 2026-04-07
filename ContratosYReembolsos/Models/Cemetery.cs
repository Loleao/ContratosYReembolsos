using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Cemetery
    {
        [Key]
        public int Id { get; set; } // Sugerencia: Usar un ID descriptivo o Identity

        public string RUC { get; set; }

        public string Name { get; set; }

        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        // --- 1. DATOS DE CONTACTO Y UBICACIÓN ---
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // --- 2. METADATOS PARA LA VISTA (INDEX) ---
        public bool IsActive { get; set; } = true;

        // Esto permite que el Cementerio "sepa" cuántas estructuras tiene
        public virtual ICollection<IntermentStructure>? Structures { get; set; }
    }
}
