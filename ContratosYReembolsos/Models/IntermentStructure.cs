using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class IntermentStructure
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Ej: "Pabellón San Judas Tadeo"

        public string Type { get; set; } // Se copia del Template (PABELLON, COLUMBARIO, TUMBA)

        [Required]
        public string Status { get; set; } = "ACTIVO";

        // Ubicación
        public int CemeteryId { get; set; } // FK a tu tabla de Sedes/Cementerios
        [ForeignKey("CemeteryId")]
        public virtual Cemetery? Cemetery { get; set; }

        public int? TemplateId { get; set; }
        public virtual IntermentStructureTemplate? Template { get; set; }

        // Relación con los nichos individuales
        public virtual ICollection<IntermentSpace> Spaces { get; set; } = new List<IntermentSpace>();
    }
}