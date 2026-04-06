using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class ContractMovilityDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        [Required]
        public string ServiceType { get; set; } = TipoMovilidad.Recojo;

        public bool IsDispatched { get; set; } = false;

        public DateTime? ScheduledDate { get; set; }
    }
}
