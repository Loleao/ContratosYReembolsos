using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;

namespace ContratosYReembolsos.Models.ViewModels.Inventory
{
    public class TransferFormViewModel
    {
        public bool IsAdmin { get; set; }
        public int? UserBranchId { get; set; }
        public string? UserBranchName { get; set; }
        public List<Branch> AllBranches { get; set; } = new();
        public List<ProductStock> AvailableStock { get; set; } // Lo que hay en stock para enviar
        public List<FixedAsset> AvailableAssets { get; set; } // Activos disponibles en Central
    }
}
