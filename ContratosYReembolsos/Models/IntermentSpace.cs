using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class IntermentSpace
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; } // Ej: SJT-P1-FA-C25

        // Coordenadas físicas
        public int FloorNumber { get; set; } = 1;
        public string RowLetter { get; set; } // A, B, C, D...
        public int ColumnNumber { get; set; } // 1 al 50

        [Required]
        public string Status { get; set; } = IntermentStatus.Disponible;

        public decimal Price { get; set; }

        // Relación con el Pabellón Padre
        public int StructureId { get; set; }
        public virtual IntermentStructure Structure { get; set; }

        // Relación con el Contrato (quién ocupa el espacio)
        public int? ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        // Datos de Inhumación (Opcional pero recomendado)
        public DateTime? InhumationDate { get; set; }
    }
}