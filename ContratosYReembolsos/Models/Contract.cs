using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string ContractNumber { get; set; } // Ejemplo: LIM-2026-0001

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Finalizado";

        // --- VÍNCULO CON LA FILIAL ---
        [Required]
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        // --- SOLICITANTE Y FALLECIDO ---
        public string SolicitorDni { get; set; }
        public string SolicitorName { get; set; }
        public string SolicitorCip { get; set; }
        public string SolicitorType { get; set; }

        public string DeceasedDni { get; set; }
        public string DeceasedName { get; set; }
        public DateTime DeathDate { get; set; }

        // --- LOGÍSTICA DE SEPULCRO ---
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }
        public string UbigeoFull { get; set; }

        // Referencia al Nicho específico en el inventario
        public int? IntermentSpaceId { get; set; }
        [ForeignKey("IntermentSpaceId")]
        public virtual IntermentSpace? IntermentSpace { get; set; }

        // Respaldo de texto (Denormalización)
        public string BurialDetail { get; set; } // Ej: "Pabellón A - Fila 2 - Nro 15"

        // --- AGENCIA (RELACIÓN REAL) ---
        public int AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public virtual Agency? Agency { get; set; }

        // --- FINANZAS ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // --- RELACIONES HIJAS ---
        public virtual ICollection<ContractDetail> Details { get; set; } = new List<ContractDetail>();
        public virtual ICollection<ContractMovilityDetail> MovilityDetails { get; set; } = new List<ContractMovilityDetail>();
    }
}