using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels.Inventory;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public InventoryService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardSummary()
        {
            return new AdminDashboardViewModel
            {
                TotalProducts = await _context.Productos.CountAsync(),
                // Se reporta 0 o se puede eliminar la propiedad del ViewModel si ya no se usará
                TotalAssets = 0,
                PendingTransfers = await _context.ProductosTransferencias.CountAsync(t => t.Status == TransferStatus.Sent),
                LowStockItems = await _context.ProductosStock.CountAsync(s => s.Quantity <= s.MinimumStock),
                Branches = await _context.Filiales.ToListAsync()
            };
        }

        public async Task<List<Product>> GetProductCatalog()
        {
            return await _context.Productos
                .Include(p => p.SubCategory)
                    .ThenInclude(s => s.Category)
                .ToListAsync();
        }

        public async Task<List<ProductCategory>> GetCategories()
        {
            return await _context.ProductosCategorias.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<ProductSubcategory>> GetSubcategoriesByCategory(int categoryId)
        {
            return await _context.ProductosSubcategorias
                .Where(s => s.CategoryId == categoryId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<(bool success, string message, string sku)> SaveProduct(Product model)
        {
            var subCategory = await _context.ProductosSubcategorias
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == model.SubCategoryId);

            if (subCategory == null) return (false, "Subcategoría inválida.", null);

            model.CategoryId = subCategory.CategoryId;

            if (model.Id == 0)
            {
                string catPrefix = (subCategory.Category.Name.Length >= 3 ? subCategory.Category.Name.Substring(0, 3) : subCategory.Category.Name).ToUpper();
                string subPrefix = (subCategory.Name.Length >= 3 ? subCategory.Name.Substring(0, 3) : subCategory.Name).ToUpper();

                int nextCount = await _context.Productos.CountAsync(p => p.SubCategoryId == model.SubCategoryId) + 1;
                model.Sku = $"{catPrefix}{model.CategoryId}-{subPrefix}{model.SubCategoryId}-{nextCount:D6}";

                _context.Productos.Add(model);
            }
            else
            {
                _context.Update(model);
            }

            await _context.SaveChangesAsync();
            return (true, "Producto guardado", model.Sku);
        }

        public async Task<(bool success, string message)> DeleteProduct(int id)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null) return (false, "El producto no existe.");

            if (await _context.ProductosStock.AnyAsync(s => s.ProductId == id && s.Quantity > 0))
                return (false, "No se puede eliminar: El producto tiene existencias registradas.");

            _context.Productos.Remove(product);
            await _context.SaveChangesAsync();
            return (true, "Producto eliminado correctamente.");
        }

        public async Task<string> GenerateInternalCode()
        {
            var year = DateTime.Now.Year;
            var count = await _context.MovimientosInventario
                .CountAsync(m => m.CreatedAt.Year == year && m.Concept == Concept.Buy);
            return $"NI-{year}-{(count + 1):D5}";
        }

        public async Task<(bool success, string message)> ProcessBulkEntry(BulkEntryViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model.Items)
                {
                    // Ignoramos item.IsAsset ya que el controlador y JS ya no lo envían
                    var stock = await _context.ProductosStock
                        .FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == item.ProductId);

                    decimal cantAnterior = stock?.Quantity ?? 0;

                    if (stock == null)
                    {
                        stock = new ProductStock { BranchId = model.BranchId, ProductId = item.ProductId, Quantity = item.Quantity };
                        _context.ProductosStock.Add(stock);
                    }
                    else stock.Quantity += item.Quantity;

                    await _context.SaveChangesAsync();

                    _context.MovimientosInventario.Add(new InventoryMovement
                    {
                        ProductId = item.ProductId,
                        BranchId = model.BranchId,
                        ProductStockId = stock.Id,
                        Quantity = item.Quantity,
                        PreviousQuantity = cantAnterior,
                        NewQuantity = stock.Quantity,
                        Concept = Concept.Buy,
                        MovementType = MovementType.Entry,
                        InternalControlNumber = model.InternalControlNumber,
                        ExternalDocumentNumber = model.ExternalDocumentNumber,
                        Description = model.Description,
                        UserId = userId,
                        CreatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Ingreso procesado correctamente");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message, string finalCode)> ProcessTransfer(TransferEntryViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = new ProductTransfer
                {
                    InternalControlNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8),
                    OriginBranchId = model.OriginBranchId,
                    TargetBranchId = model.TargetBranchId,
                    Status = TransferStatus.Sent,
                    Observation = model.Observation,
                    SentAt = DateTime.Now,
                    SentByUserId = userId,
                    Details = new List<ProductTransferDetail>()
                };

                _context.ProductosTransferencias.Add(transfer);
                await _context.SaveChangesAsync();

                string finalCode = $"TR-{DateTime.Now.Year}-{transfer.Id:D5}";
                transfer.InternalControlNumber = finalCode;

                foreach (var item in model.Items)
                {
                    var stock = await _context.ProductosStock
                        .FirstOrDefaultAsync(s => s.BranchId == model.OriginBranchId && s.ProductId == item.ProductId);

                    if (stock == null || stock.Quantity < item.Quantity)
                        throw new Exception($"Stock insuficiente en origen para el producto ID: {item.ProductId}");

                    decimal ant = stock.Quantity;
                    stock.Quantity -= item.Quantity;

                    transfer.Details.Add(new ProductTransferDetail { ProductId = item.ProductId, Quantity = item.Quantity });

                    _context.MovimientosInventario.Add(new InventoryMovement
                    {
                        ProductId = item.ProductId,
                        BranchId = model.OriginBranchId,
                        ProductStockId = stock.Id,
                        Quantity = item.Quantity,
                        PreviousQuantity = ant,
                        NewQuantity = stock.Quantity,
                        Concept = Concept.Transfer,
                        MovementType = MovementType.Exit,
                        InternalControlNumber = finalCode,
                        TransferId = transfer.Id,
                        Description = $"Salida por Transferencia {finalCode}",
                        UserId = userId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Transferencia iniciada", finalCode);
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message, null); }
        }

        public async Task<(bool success, string message)> ConfirmTransferReceipt(int transferId, string observation, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ProductosTransferencias
                    .Include(t => t.Details)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null) return (false, "Transferencia no encontrada");

                transfer.Status = TransferStatus.Received;
                transfer.ReceivedAt = DateTime.Now;
                transfer.ReceivedByUserId = userId;
                transfer.ReceptionObservation = observation;

                foreach (var det in transfer.Details)
                {
                    var stock = await _context.ProductosStock
                        .FirstOrDefaultAsync(s => s.BranchId == transfer.TargetBranchId && s.ProductId == det.ProductId);

                    decimal ant = stock?.Quantity ?? 0;

                    if (stock == null)
                    {
                        stock = new ProductStock { BranchId = transfer.TargetBranchId, ProductId = det.ProductId, Quantity = det.Quantity };
                        _context.ProductosStock.Add(stock);
                    }
                    else stock.Quantity += det.Quantity;

                    await _context.SaveChangesAsync();

                    _context.MovimientosInventario.Add(new InventoryMovement
                    {
                        ProductId = det.ProductId,
                        BranchId = transfer.TargetBranchId,
                        ProductStockId = stock.Id,
                        Quantity = det.Quantity,
                        PreviousQuantity = ant,
                        NewQuantity = stock.Quantity,
                        Concept = Concept.Transfer,
                        MovementType = MovementType.Entry,
                        InternalControlNumber = transfer.InternalControlNumber,
                        TransferId = transfer.Id,
                        Description = $"Recepción de Transferencia {transfer.InternalControlNumber}",
                        UserId = userId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Mercadería recibida correctamente");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> CancelTransfer(int id, string reason, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ProductosTransferencias
                    .Include(t => t.Details)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transfer == null) return (false, "Transferencia no encontrada");

                foreach (var det in transfer.Details)
                {
                    var stock = await _context.ProductosStock
                        .FirstOrDefaultAsync(s => s.BranchId == transfer.OriginBranchId && s.ProductId == det.ProductId);

                    if (stock != null) stock.Quantity += det.Quantity;

                    _context.MovimientosInventario.Add(new InventoryMovement
                    {
                        ProductId = det.ProductId,
                        BranchId = transfer.OriginBranchId,
                        Quantity = det.Quantity,
                        Concept = Concept.Adjustment,
                        MovementType = MovementType.Entry,
                        InternalControlNumber = transfer.InternalControlNumber,
                        Description = $"ANULACIÓN DE TR: {reason}",
                        UserId = userId
                    });
                }

                transfer.Status = TransferStatus.Cancelled;
                transfer.ReceptionObservation = $"ANULADA: {reason}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Transferencia anulada");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> SaveCategory(ProductCategory model)
        {
            if (model.Id == 0) _context.ProductosCategorias.Add(model);
            else _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return (true, "Categoría guardada");
        }

        public async Task<(bool success, string message)> SaveSubcategory(ProductSubcategory model)
        {
            if (model.Id == 0) _context.ProductosSubcategorias.Add(model);
            else _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return (true, "Subcategoría guardada");
        }

        public async Task<(bool success, string message)> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model)
        {
            if (model == null || !model.Any()) return (false, "No hay datos");
            foreach (var item in model)
            {
                var stock = await _context.ProductosStock.FindAsync(item.Id);
                if (stock != null) stock.MinimumStock = item.MinimumStock;
            }
            await _context.SaveChangesAsync();
            return (true, "Stock mínimo actualizado");
        }
    }
}