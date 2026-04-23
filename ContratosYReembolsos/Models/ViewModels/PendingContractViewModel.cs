using ContratosYReembolsos.Models.Entities.Contracts;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class PendingContractViewModel
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string DeceasedName { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }
        // Aquí solo enviaremos los detalles que REALMENTE están pendientes
        public List<ContractMovilityDetail> PendingDetails { get; set; }
    }
}
