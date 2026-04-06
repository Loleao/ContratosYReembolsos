using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class IntermentStructureTemplate
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // Ej: "Pabellón Estándar 6 Filas"

        [Required]
        public string Type { get; set; } // PABELLON, COLUMBARIO, TUMBA

        // Parámetros de construcción
        public int TotalFloors { get; set; } = 1;
        public int RowsCount { get; set; }    // 4 o 6 filas
        public int ColsPerFace { get; set; }  // 25 columnas
        public bool IsDoubleFace { get; set; } = true;

        public decimal DefaultPrice { get; set; }

        // Relación con las estructuras reales creadas con este molde
        public virtual ICollection<IntermentStructure> CreatedStructures { get; set; } = new List<IntermentStructure>();
    }
}