using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels.Inventory;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        public async Task<IEnumerable<IGrouping<string, Branch>>> GetBranchesGroupedByRegionAsync()
        {
            var branches = await _context.Filiales
                .Include(b => b.Ubigeo)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return branches
                .GroupBy(b => b.Ubigeo?.Region ?? "SIN REGIÓN")
                .OrderBy(g => g.Key);
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
            return await _context.ProductosCategorias
                .Include(c => c.SubCategories) // Importante cargar las subcategorías
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ProductCategory> GetCategoryById(int id)
        {
            return await _context.ProductosCategorias
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ProductSubcategory?> GetSubCategoryById(int id)
        {
            return await _context.ProductosSubcategorias
                .Include(s => s.Category) // Incluimos la categoría para que el formulario sepa a cuál pertenece
                .FirstOrDefaultAsync(s => s.Id == id);
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



        public async Task<Branch?> GetBranchByIdAsync(int id) => await _context.Filiales.FindAsync(id);

        public async Task<List<InventoryMovement>> GetKardexByBranchAsync(int branchId) =>
            await _context.MovimientosInventario
                .Include(m => m.Product)
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.CreatedAt).Take(100).ToListAsync();

        public async Task<string> GetNextTransferCodeAsync()
        {
            var year = DateTime.Now.Year;
            var count = await _context.ProductosTransferencias.CountAsync(t => t.SentAt.Year == year);
            return $"TR-{year}-{(count + 1):D5}";
        }

        // Trasladamos la lógica del Dashboard del Controller al Service
        public async Task<InventoryDashboardViewModel> GetDashboardDataAsync(int? branchId, int? productId, string period, string date)
        {
            var model = new InventoryDashboardViewModel
            {
                SelectedBranchId = branchId,
                SelectedProductId = productId,
                SelectedPeriod = period ?? "monthly"
            };

            DateTime refDate = DateTime.Now;
            DateTime startDate, endDate;

            // 1. Parseo de Fechas según el Período seleccionado
            if (!string.IsNullOrEmpty(date))
            {
                try
                {
                    if (period == "weekly" && date.Contains("-W"))
                    {
                        var parts = date.Split("-W");
                        refDate = System.Globalization.ISOWeek.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), DayOfWeek.Monday);
                    }
                    else if (period == "monthly" && date.Length == 7) refDate = DateTime.ParseExact(date, "yyyy-MM", null);
                    else if (period == "yearly") refDate = new DateTime(int.Parse(date), 1, 1);
                    else DateTime.TryParse(date, out refDate);
                }
                catch { refDate = DateTime.Now; }
            }
            model.DateValue = date ?? (period == "monthly" ? refDate.ToString("yyyy-MM") : refDate.ToString("yyyy-MM-dd"));

            // 2. Configuración de Rango y Labels para Gráficos
            List<string> labels = new List<string>();
            switch (model.SelectedPeriod.ToLower())
            {
                case "daily":
                    startDate = refDate.Date;
                    endDate = startDate.AddDays(1).AddTicks(-1);
                    labels = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToList();
                    break;
                case "weekly":
                    int diff = (7 + (refDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = refDate.AddDays(-1 * diff).Date;
                    endDate = startDate.AddDays(7).AddTicks(-1);
                    labels = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
                    break;
                case "yearly":
                    startDate = new DateTime(refDate.Year, 1, 1);
                    endDate = startDate.AddYears(1).AddDays(-1);
                    labels = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).Select(m => m.ToUpper()).ToList();
                    break;
                default: // Monthly
                    startDate = new DateTime(refDate.Year, refDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddTicks(-1);
                    labels = Enumerable.Range(1, DateTime.DaysInMonth(refDate.Year, refDate.Month)).Select(d => $"Día {d}").ToList();
                    break;
            }

            // 3. Carga de Datos Maestros para Filtros
            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            var productosQuery = _context.Productos.AsQueryable();
            if (branchId.HasValue)
            {
                var idsEnSede = await _context.ProductosStock.Where(s => s.BranchId == branchId).Select(s => s.ProductId).Distinct().ToListAsync();
                productosQuery = productosQuery.Where(p => idsEnSede.Contains(p.Id));
            }
            model.Productos = await productosQuery.OrderBy(p => p.Name).ToListAsync();

            // 4. Consulta de Movimientos
            var movimientos = await _context.MovimientosInventario.Include(m => m.Product)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .Where(m => !branchId.HasValue || m.BranchId == branchId.Value)
                .Where(m => !productId.HasValue || m.ProductId == productId.Value)
                .ToListAsync();

            // 5. Agregación para el Gráfico
            List<decimal> valoresConsumo = new List<decimal>();
            List<decimal> valoresIngreso = new List<decimal>();

            for (int i = 0; i < labels.Count; i++)
            {
                var temp = movimientos.Where(m =>
                    (period == "daily" && m.CreatedAt.Hour == i) ||
                    (period == "weekly" && ((int)m.CreatedAt.DayOfWeek + 6) % 7 == i) ||
                    (period == "yearly" && m.CreatedAt.Month == (i + 1)) ||
                    (period == "monthly" && m.CreatedAt.Day == (i + 1)));

                valoresConsumo.Add(temp.Where(m => m.MovementType == MovementType.Exit).Sum(x => x.Quantity));
                valoresIngreso.Add(temp.Where(m => m.MovementType == MovementType.Entry).Sum(x => x.Quantity));
            }

            model.GraficoLabelsJson = JsonConvert.SerializeObject(labels);
            model.ValoresConsumoJson = JsonConvert.SerializeObject(valoresConsumo);
            model.ValoresIngresoJson = JsonConvert.SerializeObject(valoresIngreso);
            model.TotalConsumo = valoresConsumo.Sum();
            model.TotalIngreso = valoresIngreso.Sum();

            // 6. Detalle de Tabla de Movimientos
            model.DetalleMovimientos = movimientos.GroupBy(m => new { m.ProductId, m.Product.Name, m.Product.Sku })
                .Select(g => new LineaConsumoViewModel
                {
                    ProductName = g.Key.Name,
                    Sku = g.Key.Sku,
                    StockInicial = g.OrderBy(m => m.CreatedAt).First().PreviousQuantity,
                    StockFinal = g.OrderByDescending(m => m.CreatedAt).First().NewQuantity,
                    TotalIngresos = g.Where(m => m.MovementType == MovementType.Entry).Sum(m => m.Quantity),
                    TotalConsumo = g.Where(m => m.MovementType == MovementType.Exit).Sum(m => m.Quantity)
                }).ToList();

            return model;
        }

        public async Task<ReporteGuiaViewModel?> GetReporteGuiaAsync(string controlNumber)
        {
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Where(m => m.InternalControlNumber == controlNumber)
                .ToListAsync();

            if (!movimientos.Any()) return null;

            var primerMov = movimientos.First();

            var model = new ReporteGuiaViewModel
            {
                NumeroGuia = controlNumber,
                Fecha = primerMov.CreatedAt,
                UsuarioResponsable = primerMov.UserId,
                DocumentoExterno = primerMov.ExternalDocumentNumber ?? "S/N",
                Items = movimientos.Select(m => new DetalleGuiaItem
                {
                    Producto = m.Product?.Name,
                    Cantidad = m.Quantity,
                    Tipo = m.MovementType == MovementType.Entry ? "ENTRADA" : "SALIDA",
                    Sede = m.Branch?.Name,
                    CodigoPatrimonial = "---"
                }).ToList()
            };

            // Determinamos el título y origen/destino basado en el concepto
            if (primerMov.Concept == Concept.Buy)
            {
                model.TipoOperacion = "ACTA DE RECEPCIÓN E INTERNAMIENTO";
                model.SedeOrigen = "PROVEEDOR / COMPRA";
                model.SedeDestino = primerMov.Branch?.Name;
            }
            else if (primerMov.Concept == Concept.Transfer)
            {
                model.TipoOperacion = "GUÍA DE REMISIÓN INTERNA (TRANSFERENCIA)";
                model.SedeOrigen = movimientos.FirstOrDefault(m => m.MovementType == MovementType.Exit)?.Branch?.Name;

                // Buscamos la transferencia asociada para conocer el destino final
                var transferencia = await _context.ProductosTransferencias
                    .Include(t => t.TargetBranch)
                    .FirstOrDefaultAsync(t => t.InternalControlNumber == controlNumber);

                model.SedeDestino = transferencia?.TargetBranch?.Name;
            }
            else if (primerMov.Concept == Concept.Adjustment)
            {
                model.TipoOperacion = "AJUSTE DE INVENTARIO";
                model.SedeOrigen = "SISTEMA";
                model.SedeDestino = primerMov.Branch?.Name;
            }

            return model;
        }


        public async Task<List<Branch>> GetAllBranchesAsync() =>
        await _context.Filiales.OrderBy(b => b.Name).ToListAsync();

        public async Task<List<Product>> GetAllProductsAsync() =>
            await _context.Productos.OrderBy(p => p.Name).ToListAsync();

        public async Task<List<ProductSubcategory>> GetSubcategoriesWithCategoryAsync() =>
            await _context.ProductosSubcategorias.Include(s => s.Category).ToListAsync();

        public async Task<List<ProductStock>> GetStockByBranchAsync(int branchId) =>
            await _context.ProductosStock.Include(s => s.Product)
                .Where(s => s.BranchId == branchId).ToListAsync();

        public async Task<List<ProductTransfer>> GetPendingTransfersByBranchAsync(int branchId) =>
            await _context.ProductosTransferencias
                .Include(t => t.OriginBranch)
                .Include(t => t.Details).ThenInclude(d => d.Product)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
                .ToListAsync();

        public async Task<object> GetAvailableItemsJsonByBranchAsync(int branchId)
        {
            return await _context.ProductosStock
                .Include(s => s.Product)
                .Where(s => s.BranchId == branchId && s.Quantity > 0)
                .Select(s => new {
                    productId = s.ProductId,
                    name = s.Product.Name,
                    quantity = s.Quantity,
                    unit = (int)s.Product.Unit
                }).ToListAsync();
        }

        public async Task<List<InventoryMovement>> GetAllMovementsAsync() =>
            await _context.MovimientosInventario.Include(m => m.Product).Include(m => m.Branch)
                .OrderByDescending(m => m.CreatedAt).ToListAsync();

        public async Task<List<InventoryMovement>> GetFilteredKardexAsync(int? branchId, int? productId, DateTime? start, DateTime? end)
        {
            var query = _context.MovimientosInventario.Include(m => m.Branch).Include(m => m.Product).AsQueryable();

            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);
            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
            if (start.HasValue) query = query.Where(m => m.CreatedAt.Date >= start.Value.Date);
            if (end.HasValue) query = query.Where(m => m.CreatedAt.Date <= end.Value.Date);

            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        }
    }
}