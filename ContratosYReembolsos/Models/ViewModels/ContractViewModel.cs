using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class ContractViewModel
    {
        public int BranchId { get; set; }
        public SolicitorData Solicitor { get; set; }
        public DeceasedData Deceased { get; set; }
        public int AgencyId { get; set; }
        public int CoffinVariantId { get; set; }
        public List<int> RequiredVehicles { get; set; }
        public decimal TotalAmount { get; set; }
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