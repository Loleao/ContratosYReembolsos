using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Models.ViewModels.Assets;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IAssetService
    {
        // Categorías y Subcategorías
        Task<List<AssetCategory>> GetCategoriesWithSub();
        Task<AssetCategory> GetCategoryById(int id);
        Task<bool> SaveCategory(AssetCategory model);
        Task<List<AssetSubcategory>> GetSubcategories();
        Task<AssetSubcategory> GetSubCategoryById(int id);
        Task<bool> SaveSubCategory(AssetSubcategory model);

        // Catálogo (Moldes)
        Task<List<AssetCatalog>> GetCatalog();
        Task<AssetCatalog> GetCatalogItemById(int id);
        Task<bool> SaveCatalogItem(AssetCatalog model);

        // Activos Físicos (Instancias)
        Task<List<FixedAsset>> GetAssetsByBranch(int branchId);
        Task<string> GeneratePatrimonialCode(int subcategoryId);
        Task<(bool success, string message)> ProcessBulkAssetEntry(List<FixedAsset> assets, int branchId, string observation, string userId);

        Task<string> GenerateTransferCode();
        Task<List<FixedAsset>> GetAvailableAssetsByBranch(int branchId);
        Task<(bool success, string message, string transferCode)> ProcessAssetTransfer(AssetTransferFormViewModel model, string userId);
        Task<List<AssetTransfer>> GetPendingTransfersByBranch(int targetBranchId);
        Task<(bool success, string message)> ConfirmAssetReceipt(int transferId, string observation, string userId);
        Task<List<AssetMovement>> GetKardexByBranch(int branchId); // Historial de movimientos
    }
}