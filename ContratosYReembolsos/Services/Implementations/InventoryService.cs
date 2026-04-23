using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels;
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
                TotalAssets = await _context.ActivosFijos.CountAsync(),
                PendingTransfers = await _context.ProductosTransferencias.CountAsync(t => t.Status == TransferStatus.Sent),
                LowStockItems = await _context.ProductosStock.CountAsync(s => s.Quantity <= 5),
                Branches = await _context.Filiales.ToListAsync()
            };
        }

        public async Task<List<Product>> GetProductCatalog()
        {
            return await _context.Productos.Include(p => p.SubCategory).ThenInclude(s => s.Category).ToListAsync();
        }

        public async Task<List<ProductCategory>> GetCategories()
        {
            return await _context.ProductosCategorias.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<ProductSubcategory>> GetSubcategoriesByCategory(int categoryId)
        {
            return await _context.ProductosSubcategorias.Where(s => s.CategoryId == categoryId).OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<(bool success, string message, string sku)> SaveProduct(Product model)
        {
            var subCategory = await _context.ProductosSubcategorias.Include(sc => sc.Category).FirstOrDefaultAsync(sc => sc.Id == model.SubCategoryId);
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
            var count = await _context.MovimientosInventario.CountAsync(m => m.CreatedAt.Year == year && m.Concept == Concept.Buy);
            return $"NI-{year}-{(count + 1):D5}";
        }

        public async Task<string> GeneratePatrimonialCode(int productId)
        {
            var product = await _context.Productos.Include(p => p.SubCategory).ThenInclude(s => s.Category).FirstOrDefaultAsync(p => p.Id == productId);
            if (product?.SubCategory?.Category == null) return "FONGEN0000001";

            string catPrefix = (product.SubCategory.Category.Name.Length >= 3 ? product.SubCategory.Category.Name.Substring(0, 3) : product.SubCategory.Category.Name).ToUpper();
            string prefix = $"FON{catPrefix}{product.SubCategory.CategoryId}{DateTime.Now.Year}";

            var lastCode = await _context.ActivosFijos.Where(a => a.PatrimonialCode.StartsWith(prefix)).OrderByDescending(a => a.PatrimonialCode).Select(a => a.PatrimonialCode).FirstOrDefaultAsync();
            int nextNumber = lastCode != null && int.TryParse(lastCode.Replace(prefix, ""), out int lastNum) ? lastNum + 1 : 1;

            return $"{prefix}{nextNumber:D5}";
        }

        public async Task<(bool success, string message)> ProcessBulkEntry(BulkEntryViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model.Items)
                {
                    int finalProductId = item.IsAsset ? item.ProductIdAsset.Value : item.ProductId;
                    if (item.IsAsset)
                    {
                        var product = await _context.Productos.Include(p => p.SubCategory).ThenInclude(sc => sc.Category).FirstOrDefaultAsync(p => p.Id == finalProductId);
                        string catPart = $"{(product.SubCategory.Category.Name.Length >= 3 ? product.SubCategory.Category.Name.Substring(0, 3) : product.SubCategory.Category.Name).ToUpper()}{product.SubCategory.Category.Id}";
                        string subPart = $"{(product.SubCategory.Name.Length >= 3 ? product.SubCategory.Name.Substring(0, 3) : product.SubCategory.Name).ToUpper()}{product.SubCategory.Id}";
                        string prefixFilter = $"FON-{DateTime.Now.Year}-{catPart}-{subPart}-";

                        var lastAsset = await _context.ActivosFijos.Where(a => a.PatrimonialCode.StartsWith(prefixFilter)).OrderByDescending(a => a.PatrimonialCode).FirstOrDefaultAsync();
                        int nextNumber = lastAsset != null && int.TryParse(lastAsset.PatrimonialCode.Split('-').Last(), out int lastNum) ? lastNum + 1 : 1;

                        for (int i = 0; i < item.Quantity; i++)
                        {
                            var newAsset = new FixedAsset { ProductId = finalProductId, BranchId = model.BranchId, SerialNumber = item.SerialNumber ?? "S/N", PatrimonialCode = $"{prefixFilter}{nextNumber:D5}", Status = "Available", CreatedAt = DateTime.Now };
                            _context.ActivosFijos.Add(newAsset);
                            await _context.SaveChangesAsync();

                            _context.MovimientosInventario.Add(new InventoryMovement { ProductId = finalProductId, BranchId = model.BranchId, FixedAssetId = newAsset.Id, Quantity = 1, Concept = Concept.Buy, MovementType = MovementType.Entry, InternalControlNumber = model.InternalControlNumber, ExternalDocumentNumber = model.ExternalDocumentNumber, Description = $"{model.Description} (Cód: {newAsset.PatrimonialCode})", UserId = userId, CreatedAt = DateTime.Now });
                            nextNumber++;
                        }
                    }
                    else
                    {
                        var stock = await _context.ProductosStock.FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == finalProductId);
                        decimal cantAnterior = stock?.Quantity ?? 0;
                        if (stock == null) { stock = new ProductStock { BranchId = model.BranchId, ProductId = finalProductId, Quantity = item.Quantity }; _context.ProductosStock.Add(stock); }
                        else stock.Quantity += item.Quantity;
                        await _context.SaveChangesAsync();

                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = finalProductId, BranchId = model.BranchId, ProductStockId = stock.Id, Quantity = item.Quantity, PreviousQuantity = cantAnterior, NewQuantity = stock.Quantity, Concept = Concept.Buy, MovementType = MovementType.Entry, InternalControlNumber = model.InternalControlNumber, ExternalDocumentNumber = model.ExternalDocumentNumber, Description = model.Description, UserId = userId, CreatedAt = DateTime.Now });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Procesado");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> RegisterEntry(int productId, int branchId, int quantity, string? observation, string? serialNumber, string userId)
        {
            var product = await _context.Productos.FindAsync(productId);
            if (product == null) return (false, "No encontrado");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (product.ControlType == ControlType.Stock)
                {
                    var stock = await _context.ProductosStock.FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);
                    if (stock == null) { stock = new ProductStock { ProductId = productId, BranchId = branchId, Quantity = quantity }; _context.ProductosStock.Add(stock); }
                    else stock.Quantity += quantity;
                    await _context.SaveChangesAsync();
                    _context.MovimientosInventario.Add(new InventoryMovement { ProductId = productId, BranchId = branchId, ProductStockId = stock.Id, Quantity = quantity, MovementType = MovementType.Entry, Description = observation ?? "Ingreso", CreatedAt = DateTime.Now, UserId = userId });
                }
                else
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        var asset = new FixedAsset { ProductId = productId, BranchId = branchId, SerialNumber = serialNumber ?? "S/N", PatrimonialCode = await GeneratePatrimonialCode(productId), Status = "Available" };
                        _context.ActivosFijos.Add(asset); await _context.SaveChangesAsync();
                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = productId, BranchId = branchId, FixedAssetId = asset.Id, Quantity = 1, MovementType = MovementType.Entry, Description = $"Alta | {observation}", CreatedAt = DateTime.Now, UserId = userId });
                    }
                }
                await _context.SaveChangesAsync(); await transaction.CommitAsync(); return (true, "OK");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message, string finalCode)> ProcessTransfer(TransferEntryViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = new ProductTransfer { InternalControlNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8), OriginBranchId = model.OriginBranchId, TargetBranchId = model.TargetBranchId, Status = TransferStatus.Sent, Observation = model.Observation, SentAt = DateTime.Now, SentByUserId = userId, Details = new List<ProductTransferDetail>() };
                _context.ProductosTransferencias.Add(transfer); await _context.SaveChangesAsync();
                string finalCode = $"TR-{DateTime.Now.Year}-{transfer.Id:D5}"; transfer.InternalControlNumber = finalCode;

                foreach (var item in model.Items)
                {
                    if (item.IsAsset)
                    {
                        var asset = await _context.ActivosFijos.FindAsync(item.FixedAssetId);
                        asset.Status = "InTransit";
                        transfer.Details.Add(new ProductTransferDetail { ProductId = item.ProductId, FixedAssetId = item.FixedAssetId, Quantity = 1 });
                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = item.ProductId, BranchId = model.OriginBranchId, FixedAssetId = item.FixedAssetId, Quantity = 1, PreviousQuantity = 1, NewQuantity = 0, Concept = Concept.Transfer, MovementType = MovementType.Exit, InternalControlNumber = finalCode, TransferId = transfer.Id, Description = $"Salida TR {finalCode}", UserId = userId });
                    }
                    else
                    {
                        var stock = await _context.ProductosStock.FirstOrDefaultAsync(s => s.BranchId == model.OriginBranchId && s.ProductId == item.ProductId);
                        decimal ant = stock.Quantity; stock.Quantity -= item.Quantity;
                        transfer.Details.Add(new ProductTransferDetail { ProductId = item.ProductId, Quantity = item.Quantity });
                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = item.ProductId, BranchId = model.OriginBranchId, ProductStockId = stock.Id, Quantity = item.Quantity, PreviousQuantity = ant, NewQuantity = stock.Quantity, Concept = Concept.Transfer, MovementType = MovementType.Exit, InternalControlNumber = finalCode, TransferId = transfer.Id, Description = $"Salida TR {finalCode}", UserId = userId });
                    }
                }
                await _context.SaveChangesAsync(); await transaction.CommitAsync(); return (true, "OK", finalCode);
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message, null); }
        }

        public async Task<(bool success, string message)> ConfirmTransferReceipt(int transferId, string observation, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ProductosTransferencias.Include(t => t.Details).FirstOrDefaultAsync(t => t.Id == transferId);
                transfer.Status = TransferStatus.Received; transfer.ReceivedAt = DateTime.Now; transfer.ReceivedByUserId = userId; transfer.ReceptionObservation = observation;
                foreach (var det in transfer.Details)
                {
                    if (det.FixedAssetId.HasValue)
                    {
                        var asset = await _context.ActivosFijos.FindAsync(det.FixedAssetId);
                        asset.BranchId = transfer.TargetBranchId; asset.Status = "Available";
                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = det.ProductId, BranchId = transfer.TargetBranchId, FixedAssetId = det.FixedAssetId, Quantity = 1, NewQuantity = 1, Concept = Concept.Transfer, MovementType = MovementType.Entry, InternalControlNumber = transfer.InternalControlNumber, TransferId = transfer.Id, Description = $"Recepcion {transfer.InternalControlNumber}", UserId = userId });
                    }
                    else
                    {
                        var stock = await _context.ProductosStock.FirstOrDefaultAsync(s => s.BranchId == transfer.TargetBranchId && s.ProductId == det.ProductId);
                        decimal ant = stock?.Quantity ?? 0;
                        if (stock == null) { stock = new ProductStock { BranchId = transfer.TargetBranchId, ProductId = det.ProductId, Quantity = det.Quantity }; _context.ProductosStock.Add(stock); }
                        else stock.Quantity += det.Quantity;
                        await _context.SaveChangesAsync();
                        _context.MovimientosInventario.Add(new InventoryMovement { ProductId = det.ProductId, BranchId = transfer.TargetBranchId, ProductStockId = stock.Id, Quantity = det.Quantity, PreviousQuantity = ant, NewQuantity = stock.Quantity, Concept = Concept.Transfer, MovementType = MovementType.Entry, InternalControlNumber = transfer.InternalControlNumber, TransferId = transfer.Id, Description = $"Recepcion {transfer.InternalControlNumber}", UserId = userId });
                    }
                }
                await _context.SaveChangesAsync(); await transaction.CommitAsync(); return (true, "OK");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> CancelTransfer(int id, string reason, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ProductosTransferencias.Include(t => t.Details).FirstOrDefaultAsync(t => t.Id == id);
                foreach (var det in transfer.Details)
                {
                    if (det.FixedAssetId.HasValue) { var asset = await _context.ActivosFijos.FindAsync(det.FixedAssetId); asset.Status = "Disponible"; }
                    else { var stock = await _context.ProductosStock.FirstOrDefaultAsync(s => s.BranchId == transfer.OriginBranchId && s.ProductId == det.ProductId); stock.Quantity += det.Quantity; }
                    _context.MovimientosInventario.Add(new InventoryMovement { ProductId = det.ProductId, BranchId = transfer.OriginBranchId, Quantity = det.Quantity, Concept = Concept.Adjustment, MovementType = MovementType.Entry, InternalControlNumber = transfer.InternalControlNumber, Description = $"ANULACIÓN: {reason}", UserId = userId });
                }
                transfer.Status = TransferStatus.Cancelled; transfer.ReceptionObservation = $"ANULADA: {reason}";
                await _context.SaveChangesAsync(); await transaction.CommitAsync(); return (true, "OK");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> SaveCategory(ProductCategory model)
        {
            try
            {
                if (model.Id == 0) _context.ProductosCategorias.Add(model);
                else _context.Entry(model).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return (true, "Categoría guardada correctamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> SaveSubcategory(ProductSubcategory model)
        {
            try
            {
                if (model.Id == 0) _context.ProductosSubcategorias.Add(model);
                else _context.Entry(model).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return (true, "Subcategoría guardada correctamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model)
        {
            try
            {
                if (model == null || !model.Any())
                    return (false, "No hay datos para actualizar.");

                foreach (var item in model)
                {
                    var stock = await _context.ProductosStock.FindAsync(item.Id);
                    if (stock != null)
                    {
                        stock.MinimumStock = item.MinimumStock;
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Niveles de stock mínimo actualizados correctamente.");
            }
            catch (Exception ex)
            {
                return (false, "Error al actualizar stock mínimo: " + ex.Message);
            }
        }
    }
}