using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels.Inventory;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IInventoryService
    {
        // Consultas de Dashboard y Catalogos
        Task<AdminDashboardViewModel> GetAdminDashboardSummary();
        Task<List<Product>> GetProductCatalog();
        Task<List<ProductCategory>> GetCategories();
        Task<List<ProductSubcategory>> GetSubcategoriesByCategory(int categoryId);

        // Gestión de Productos
        Task<(bool success, string message, string sku)> SaveProduct(Product model);
        Task<(bool success, string message)> DeleteProduct(int id);

        // Operaciones de Stock e Internamiento
        Task<string> GenerateInternalCode();
        Task<string> GeneratePatrimonialCode(int productId);
        Task<(bool success, string message)> ProcessBulkEntry(BulkEntryViewModel model, string userId);
        Task<(bool success, string message)> RegisterEntry(int productId, int branchId, int quantity, string? observation, string? serialNumber, string userId);
        Task<(bool success, string message)> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model);

        // Transferencias
        Task<(bool success, string message, string finalCode)> ProcessTransfer(TransferEntryViewModel model, string userId);
        Task<(bool success, string message)> ConfirmTransferReceipt(int transferId, string receptionObservation, string userId);
        Task<(bool success, string message)> CancelTransfer(int id, string reason, string userId);

        // Gestión de Categorías y Subcategorías
        Task<(bool success, string message)> SaveCategory(ProductCategory model);
        Task<(bool success, string message)> SaveSubcategory(ProductSubcategory model);


    }
}