using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.FixedAssets;

namespace ContratosYReembolsos.Models.ViewModels.Assets
{
    public class AssetTransferFormViewModel
    {
        public List<Branch> AllBranches { get; set; } = new List<Branch>();
        public int? UserBranchId { get; set; }
        public string? UserBranchName { get; set; }
        public bool IsAdmin { get; set; }

        // Propiedades para recibir los datos del POST
        public int OriginBranchId { get; set; }
        public int TargetBranchId { get; set; }
        public string InternalControlNumber { get; set; }
        public string Observation { get; set; }

        // Lista de IDs de los activos físicos a trasladar
        public List<int> SelectedAssetIds { get; set; } = new List<int>();
    }
}