using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels.Inventory;

namespace ContratosYReembolsos.Services.Interfaces
{
    //public interface IInventoryService
    //{
    //    // Consultas de Dashboard y Catalogos
    //    Task<AdminDashboardViewModel> GetAdminDashboardSummary();
    //    Task<List<Product>> GetProductCatalog();
    //    Task<List<ProductCategory>> GetCategories();
    //    Task<List<ProductSubcategory>> GetSubcategoriesByCategory(int categoryId);
    //    Task<IEnumerable<IGrouping<string, Branch>>> GetBranchesGroupedByRegionAsync();

    //    // Gestión de Productos
    //    Task<(bool success, string message, string sku)> SaveProduct(Product model);
    //    Task<(bool success, string message)> DeleteProduct(int id);

    //    // Operaciones de Stock e Internamiento
    //    Task<string> GenerateInternalCode();
    //    Task<(bool success, string message)> ProcessBulkEntry(BulkEntryViewModel model, string userId);
    //    Task<(bool success, string message)> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model);

    //    // Transferencias
    //    Task<(bool success, string message, string finalCode)> ProcessTransfer(TransferEntryViewModel model, string userId);
    //    Task<(bool success, string message)> ConfirmTransferReceipt(int transferId, string receptionObservation, string userId);
    //    Task<(bool success, string message)> CancelTransfer(int id, string reason, string userId);

    //    // Gestión de Categorías y Subcategorías
    //    Task<(bool success, string message)> SaveCategory(ProductCategory model);
    //    Task<(bool success, string message)> SaveSubcategory(ProductSubcategory model);


    //}

    public interface IInventoryService
    {
        // Dashboards y Consultas de Sedes
        Task<AdminDashboardViewModel> GetAdminDashboardSummary();
        Task<IEnumerable<IGrouping<string, Branch>>> GetBranchesGroupedByRegionAsync();
        Task<Branch?> GetBranchByIdAsync(int id);
        Task<List<Branch>> GetAllBranchesAsync();

        // Catálogo y Stock
        Task<List<Product>> GetProductCatalog();
        Task<List<ProductCategory>> GetCategories();
        Task<ProductCategory> GetCategoryById(int id);

        Task<List<ProductSubcategory>> GetSubcategoriesWithCategoryAsync();
        Task<List<ProductSubcategory>> GetSubcategoriesByCategory(int categoryId);
        Task<ProductSubcategory?> GetSubCategoryById(int id);

        Task<List<ProductStock>> GetStockByBranchAsync(int branchId);
        Task<List<ProductTransfer>> GetPendingTransfersByBranchAsync(int branchId);
        Task<List<InventoryMovement>> GetKardexByBranchAsync(int branchId);

        // Operaciones
        Task<string> GenerateInternalCode();
        Task<string> GetNextTransferCodeAsync();
        Task<(bool success, string message)> ProcessBulkEntry(BulkEntryViewModel model, string userId);
        Task<(bool success, string message, string finalCode)> ProcessTransfer(TransferEntryViewModel model, string userId);
        Task<(bool success, string message)> ConfirmTransferReceipt(int transferId, string observation, string userId);
        Task<(bool success, string message)> SaveCategory(ProductCategory model);
        Task<(bool success, string message)> SaveSubcategory(ProductSubcategory model);
        Task<(bool success, string message)> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model);

        // Dashboard y Reportes
        Task<InventoryDashboardViewModel> GetDashboardDataAsync(int? branchId, int? productId, string period, string date);
        Task<ReporteGuiaViewModel?> GetReporteGuiaAsync(string controlNumber);

        // Consultas básicas
        Task<List<Product>> GetAllProductsAsync();

        // Vistas de Stock y Pendientes
        Task<object> GetAvailableItemsJsonByBranchAsync(int branchId); // Para el JSON de productos consumibles

        // Movimientos y Kardex
        Task<List<InventoryMovement>> GetAllMovementsAsync();
        Task<List<InventoryMovement>> GetFilteredKardexAsync(int? branchId, int? productId, DateTime? start, DateTime? end);

    }
}