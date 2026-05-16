using System.ComponentModel.DataAnnotations;
using ContratosYReembolsos.Models.Entities.Cemeteries;

namespace ContratosYReembolsos.Models.Entities.Cemeteries
{
    public class Deceased
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Dni { get; set; } = string.Empty;

        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }

        // Origen del registro: "CONTRATO" o "REGULARIZACION_DIRECTA"
        [Required, StringLength(50)]
        public string EntryType { get; set; } = "CONTRATO";

        // Código de trazabilidad física (Número de Contrato o Folio REG-XXXX)
        [Required, StringLength(50)]
        public string TrackingCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}