using ContratosYReembolsos.Models.Entities.FixedAssets;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IAssetService
    {
        // Catálogo
        Task<List<AssetCatalog>> GetCatalog();
        Task<bool> SaveCatalogItem(AssetCatalog model);

        // Operaciones de Activos
        Task<List<FixedAsset>> GetAssetsByBranch(int branchId);
        Task<(bool success, string message)> RegisterAssetEntry(FixedAsset model);

        // Movimientos
        Task<bool> AssignAsset(int assetId, string responsibleName, string notes);
        Task<bool> ChangeAssetStatus(int assetId, AssetStatus newStatus, string notes);

        // Helpers
        Task<string> GeneratePatrimonialCode(int subcategoryId);
        Task<List<AssetCategory>> GetCategories();
        Task<List<AssetSubcategory>> GetSubcategories();
        Task<List<AssetSubcategory>> GetSubcategoriesByCategoryId(int categoryId);
    }
}