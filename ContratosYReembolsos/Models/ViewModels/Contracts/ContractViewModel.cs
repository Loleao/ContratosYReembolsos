using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels.Contracts
{
    public class ContractViewModel
    {
        public int BranchId { get; set; }
        public SolicitorData Solicitor { get; set; }
        public DeceasedData Deceased { get; set; }
        public int AgencyId { get; set; }
        public List<int> StockItems { get; set; }      // IDs de Productos (Ataúdes)
        public List<int> AssetItems { get; set; }      // IDs de Activos Fijos (Capillas)
        public List<int> MobilityItems { get; set; }
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
        public int CemeteryId { get; set; }
        public int? StructureId { get; set; }
        public int? IntermentSpaceId { get; set; }
    }
}