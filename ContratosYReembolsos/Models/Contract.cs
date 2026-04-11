using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        // Personas
        public string SolicitorDni { get; set; }
        public string SolicitorName { get; set; }
        public string SolicitorType { get; set; }
        public string DeceasedDni { get; set; }
        public string DeceasedName { get; set; }

        // Logística
        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }

        public string UbigeoId { get; set; }
        public virtual Ubigeo? Ubigeo { get; set; }

        public int? WakeId { get; set; }

        public int CemeteryId { get; set; }
        public virtual Cemetery? Cemetery { get; set; }

        public int? IntermentStructureId { get; set; }
        public virtual IntermentStructure? IntermentStructure { get; set; }

        public int? IntermentSpaceId { get; set; }
        public virtual IntermentSpace? IntermentSpace { get; set; }

        // Servicios
        public int CoffinVariantId { get; set; }
        public virtual CoffinVariant? CoffinVariant { get; set; }

        public int AgencyId { get; set; }
        public virtual Agency? Agency { get; set; }

        public decimal TotalAmount { get; set; }

        // Relación Detallada
        public virtual ICollection<ContractMovilityDetail> MovilityDetails { get; set; }
    }
}