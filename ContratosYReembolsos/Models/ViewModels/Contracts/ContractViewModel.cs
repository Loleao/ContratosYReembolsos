
namespace ContratosYReembolsos.Models.ViewModels.Contracts
{
    public class ContractViewModel
    {
        public ContractViewModel()
        {
            // Inicializar listas para evitar NullReferenceException
            StockItems = new List<int>();
            ServiceItems = new List<int>();
            ExternalServiceItems = new List<int>();
            MobilityItems = new List<int>();
            Affiliate = new AffiliateData();
            Solicitor = new SolicitorData();
            Deceased = new DeceasedData();
        }

        public int BranchId { get; set; }
        public AffiliateData Affiliate { get; set; }
        public SolicitorData Solicitor { get; set; }
        public DeceasedData Deceased { get; set; }

        // Propiedades para Convenio
        public bool UseAgreement { get; set; } // Viene del Checkbox del Step 3
        public int? AgencyId { get; set; }     // Puede ser null si no hay convenio

        public List<int> StockItems { get; set; }
        public List<int> ServiceItems { get; set; }         // Servicios FONAFUN
        public List<int> ExternalServiceItems { get; set; } // Servicios Agencia/Convenio
        public List<int> MobilityItems { get; set; }
    }

    public class AffiliateData
    {
        public string Dni { get; set; }
        public string FullName { get; set; }
        public string Cip { get; set; }
    }

    public class SolicitorData
    {
        public string Dni { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class DeceasedData
    {
        public string Dni { get; set; }
        public string Name { get; set; }
        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public string BurialTime { get; set; }
        public string Inei { get; set; }
        public int? WakeId { get; set; }
        public string? CustomWakeAddress { get; set; }
        public int CemeteryId { get; set; }
        public int? StructureId { get; set; }
        public int? IntermentSpaceId { get; set; }
    }
}