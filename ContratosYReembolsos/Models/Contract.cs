using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        // Código generado: JUN202600001
        [Required, StringLength(20)]
        public string ContractNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Finalizado";

        // --- PASO 1: SOLICITANTE ---
        public string SolicitorDni { get; set; }
        public string SolicitorName { get; set; }
        public string SolicitorCip { get; set; }
        public string SolicitorType { get; set; } // "Titular" o "Familiar"

        // --- PASO 2: FALLECIDO ---
        public string DeceasedDni { get; set; }
        public string DeceasedName { get; set; }
        public DateTime DeathDate { get; set; }

        // --- LOGÍSTICA DE SEPULCRO ---
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }

        // Ubicación del Fallecimiento (Ubigeo)
        public string IneiCode { get; set; } // El ID del distrito seleccionado
        public string UbigeoFull { get; set; } // Texto: "JUNIN - HUANCAYO - EL TAMBO"

        // Velatorio
        public int? WakeId { get; set; }
        public string WakeName { get; set; } // Denormalizado para histórico

        // Cementerio
        public string CemeteryId { get; set; }
        public string CemeteryName { get; set; } // Denormalizado para histórico

        public string BurialType { get; set; } // "Pabellon", "Tumba", "Columbario"
        public string BurialDetail { get; set; } // "Pabellón San Juan - Fila A - Nro 5"

        // --- PASO 3: AGENCIA ---
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }

        // --- FINANZAS ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // --- RELACIONES ---
        public virtual ICollection<ContractDetail> Details { get; set; } = new List<ContractDetail>();
        public virtual ICollection<ContractMovilityDetail> MovilityDetails { get; set; } = new List<ContractMovilityDetail>();

    }
}