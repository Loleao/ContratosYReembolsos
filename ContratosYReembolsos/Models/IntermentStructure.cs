using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class IntermentStructure
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Ej: "Pabellón San Judas Tadeo"

        public string Type { get; set; } // Se copia del Template (PABELLON, COLUMBARIO, TUMBA)

        [Required]
        public string Status { get; set; } = "ACTIVO"; // ACTIVO, LLENO, MANTENIMIENTO

        // Ubicación
        public int CemeteryId { get; set; } // FK a tu tabla de Sedes/Cementerios

        // Relación con el Molde (opcional, para saber de dónde vino)
        public int? TemplateId { get; set; }
        public virtual IntermentStructureTemplate Template { get; set; }

        // Relación con los nichos individuales
        public virtual ICollection<IntermentSpace> Spaces { get; set; } = new List<IntermentSpace>();
    }
}