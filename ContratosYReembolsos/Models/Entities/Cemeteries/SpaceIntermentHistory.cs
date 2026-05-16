using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.Entities.Cemeteries
{
    public class SpaceIntermentHistory
    {
        public int Id { get; set; }

        public int IntermentSpaceId { get; set; }
        public virtual IntermentSpace? IntermentSpace { get; set; }

        [Required, StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        public int DeceasedId { get; set; }
        public virtual Deceased? Deceased { get; set; }

        public DateTime StartDate { get; set; }

        // Si es NULL, significa que el difunto sigue actualmente sepultado ahí
        public DateTime? EndDate { get; set; }

        // Ejemplo: "INHUMACIÓN ORIGINAL", "EXHUMACIÓN POR TRASLADO", "REUBICACIÓN"
        [Required, StringLength(100)]
        public string OperationType { get; set; } = "INHUMACIÓN";

        public string? Observations { get; set; }
    }
}
