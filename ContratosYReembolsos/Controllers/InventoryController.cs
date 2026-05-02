//using ContratosYReembolsos.Data.Contexts;
//using ContratosYReembolsos.Models.Entities.Inventory;
//using ContratosYReembolsos.Models.ViewModels;
//using ContratosYReembolsos.Services.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Rotativa.AspNetCore;
//using System;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using ContratosYReembolsos.Models.ViewModels.Inventory;

//namespace ContratosYReembolsos.Controllers
//{
//    public class InventoryController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IInventoryService _inventoryService;
//        private readonly INotificationService _notificationService;

//        public InventoryController(ApplicationDbContext context, IInventoryService inventoryService, INotificationService notificationService)
//        {
//            _context = context;
//            _inventoryService = inventoryService;
//            _notificationService = notificationService;
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Index() => View(await _inventoryService.GetAdminDashboardSummary());

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Catalog()
//        {
//            ViewBag.Categories = await _inventoryService.GetCategories();
//            // Cargamos subcategorías iniciales para los filtros del catálogo
//            ViewBag.SubCategories = await _context.ProductosSubcategorias.Include(s => s.Category).ToListAsync();
//            return View(await _inventoryService.GetProductCatalog());
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Categories()
//            => View(await _inventoryService.GetCategories());

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Subcategories()
//        {
//            var subs = await _context.ProductosSubcategorias.Include(s => s.Category).ToListAsync();
//            return View(subs);
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> BranchSelection()
//        {
//            var branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
//            return View(branches);
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Inventory(int branchId)
//        {
//            var sede = await _context.Filiales.FindAsync(branchId);
//            if (sede == null) return NotFound();

//            // 1. Cargamos el stock de la sede (Eliminado el filtro ControlType)
//            var stockInventario = await _context.ProductosStock
//                .Include(s => s.Product)
//                    .ThenInclude(p => p.SubCategory)
//                .Where(s => s.BranchId == branchId) // Ya no filtramos por ControlType
//                .ToListAsync();

//            // 2. Cargamos transferencias pendientes
//            var transferenciasPendientes = await _context.ProductosTransferencias
//                .Include(t => t.OriginBranch)
//                .Include(t => t.Details)
//                    .ThenInclude(d => d.Product)
//                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
//                .ToListAsync();

//            ViewBag.SedeNombre = sede.Name;
//            ViewBag.BranchId = branchId;
//            ViewBag.TransferenciasPendientes = transferenciasPendientes;

//            // Retornamos solo la lista de stock (ajusta tu Vista si recibía una Tupla antes)
//            return View(stockInventario);
//        }

//        public async Task<IActionResult> Movements()
//        {
//            var movements = await _context.MovimientosInventario
//                .Include(m => m.Product).Include(m => m.Branch)
//                .OrderByDescending(m => m.CreatedAt).ToListAsync();
//            return View(movements);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Kardex(int? branchId, int? productId, DateTime? start, DateTime? end)
//        {
//            ViewBag.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
//            ViewBag.Productos = await _context.Productos.OrderBy(p => p.Name).ToListAsync();

//            var query = _context.MovimientosInventario
//                .Include(m => m.Branch).Include(m => m.Product).Include(m => m.FixedAsset)
//                .AsQueryable();

//            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);
//            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
//            if (start.HasValue) query = query.Where(m => m.CreatedAt.Date >= start.Value.Date);
//            if (end.HasValue) query = query.Where(m => m.CreatedAt.Date <= end.Value.Date);

//            return View(await query.OrderByDescending(m => m.CreatedAt).ToListAsync());
//        }

//        [HttpGet]
//        public async Task<IActionResult> InventoryDashboard(int? branchId, int? productId, string period = "monthly", string date = null)
//        {
//            var model = new InventoryDashboardViewModel
//            {
//                SelectedBranchId = branchId,
//                SelectedProductId = productId,
//                SelectedPeriod = period ?? "monthly"
//            };

//            DateTime refDate = DateTime.Now;
//            DateTime startDate, endDate;

//            // 1. Procesamiento de Fecha
//            if (!string.IsNullOrEmpty(date))
//            {
//                try
//                {
//                    if (period == "weekly" && date.Contains("-W"))
//                    {
//                        var parts = date.Split("-W");
//                        refDate = System.Globalization.ISOWeek.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), DayOfWeek.Monday);
//                    }
//                    else if (period == "monthly" && date.Length == 7)
//                    {
//                        refDate = DateTime.ParseExact(date, "yyyy-MM", null);
//                    }
//                    else if (period == "yearly")
//                    {
//                        refDate = new DateTime(int.Parse(date), 1, 1);
//                    }
//                    else { DateTime.TryParse(date, out refDate); }
//                }
//                catch { refDate = DateTime.Now; }
//            }
//            model.DateValue = date ?? (period == "monthly" ? refDate.ToString("yyyy-MM") : refDate.ToString("yyyy-MM-dd"));

//            // 2. Configuración de Rangos y Etiquetas
//            List<string> labels = new List<string>();
//            switch (model.SelectedPeriod.ToLower())
//            {
//                case "daily":
//                    startDate = refDate.Date; endDate = startDate.AddDays(1).AddTicks(-1);
//                    labels = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToList();
//                    break;
//                case "weekly":
//                    int diff = (7 + (refDate.DayOfWeek - DayOfWeek.Monday)) % 7;
//                    startDate = refDate.AddDays(-1 * diff).Date; endDate = startDate.AddDays(7).AddTicks(-1);
//                    labels = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
//                    break;
//                case "yearly":
//                    startDate = new DateTime(refDate.Year, 1, 1); endDate = startDate.AddYears(1).AddDays(-1);
//                    labels = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).Select(m => m.ToUpper()).ToList();
//                    break;
//                default:
//                    startDate = new DateTime(refDate.Year, refDate.Month, 1); endDate = startDate.AddMonths(1).AddTicks(-1);
//                    labels = Enumerable.Range(1, DateTime.DaysInMonth(refDate.Year, refDate.Month)).Select(d => $"Día {d}").ToList();
//                    break;
//            }

//            // 3. LLENADO DE CATÁLOGOS (Eliminado filtro ControlType)
//            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();

//            // Ahora tomamos todos los productos directamente
//            var productosQuery = _context.Productos.AsQueryable();

//            if (branchId.HasValue)
//            {
//                var idsEnSede = await _context.ProductosStock
//                    .Where(s => s.BranchId == branchId)
//                    .Select(s => s.ProductId)
//                    .Distinct()
//                    .ToListAsync();
//                productosQuery = productosQuery.Where(p => idsEnSede.Contains(p.Id));
//            }
//            model.Productos = await productosQuery.OrderBy(p => p.Name).ToListAsync();

//            // 4. Consulta de Movimientos (Eliminado filtro ControlType)
//            var query = _context.MovimientosInventario.Include(m => m.Product)
//                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate);

//            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);
//            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);

//            var movimientos = await query.ToListAsync();

//            // 5. Cálculos para Chart.js
//            List<decimal> valoresConsumo = new List<decimal>();
//            List<decimal> valoresIngreso = new List<decimal>();

//            for (int i = 0; i < labels.Count; i++)
//            {
//                var temp = movimientos.Where(m =>
//                    (period == "daily" && m.CreatedAt.Hour == i) ||
//                    (period == "weekly" && ((int)m.CreatedAt.DayOfWeek + 6) % 7 == i) ||
//                    (period == "yearly" && m.CreatedAt.Month == (i + 1)) ||
//                    (period == "monthly" && m.CreatedAt.Day == (i + 1)));

//                valoresConsumo.Add(temp.Where(m => m.MovementType == MovementType.Exit).Sum(x => x.Quantity));
//                valoresIngreso.Add(temp.Where(m => m.MovementType == MovementType.Entry).Sum(x => x.Quantity));
//            }

//            model.GraficoLabelsJson = JsonConvert.SerializeObject(labels);
//            model.ValoresConsumoJson = JsonConvert.SerializeObject(valoresConsumo);
//            model.ValoresIngresoJson = JsonConvert.SerializeObject(valoresIngreso);
//            model.TotalConsumo = valoresConsumo.Sum();
//            model.TotalIngreso = valoresIngreso.Sum();

//            // 6. Llenado de la Tabla
//            model.DetalleMovimientos = movimientos.GroupBy(m => new { m.ProductId, m.Product.Name, m.Product.Sku })
//                .Select(g => new LineaConsumoViewModel
//                {
//                    ProductName = g.Key.Name,
//                    Sku = g.Key.Sku,
//                    StockInicial = g.OrderBy(m => m.CreatedAt).First().PreviousQuantity,
//                    StockFinal = g.OrderByDescending(m => m.CreatedAt).First().NewQuantity,
//                    TotalIngresos = g.Where(m => m.MovementType == MovementType.Entry).Sum(m => m.Quantity),
//                    TotalConsumo = g.Where(m => m.MovementType == MovementType.Exit).Sum(m => m.Quantity)
//                }).ToList();

//            return View(model);
//        }

//        [HttpGet]
//        public async Task<IActionResult> AssetDashboard(int? branchId, string period = "monthly", string date = null)
//        {
//            var model = new AssetDashboardViewModel
//            {
//                SelectedBranchId = branchId,
//                SelectedPeriod = period ?? "monthly"
//            };

//            // 1. Lógica de Fechas (Para mantener consistencia en el filtro)
//            DateTime refDate = DateTime.Now;
//            model.DateValue = date ?? (period == "monthly" ? refDate.ToString("yyyy-MM") : refDate.ToString("yyyy-MM-dd"));

//            // 2. LLENADO DE CATÁLOGOS (Esto soluciona el NullReferenceException de 'Sedes')
//            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();

//            // 3. Gráfico Circular: Estados de los Activos Fijos
//            var assetsQuery = _context.ActivosFijos.AsQueryable();
//            if (branchId.HasValue)
//            {
//                assetsQuery = assetsQuery.Where(a => a.BranchId == branchId);
//            }

//            var estados = await assetsQuery.GroupBy(a => a.Status)
//                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
//                .ToListAsync();

//            // Serialización para Chart.js
//            model.EstadosLabelsJson = JsonConvert.SerializeObject(estados.Select(e => e.Estado));
//            model.EstadosValoresJson = JsonConvert.SerializeObject(estados.Select(e => e.Cantidad));

//            // 4. Tabla de Resumen por Tipo de Producto (Llenado de ResumenActivos)
//            model.ResumenActivos = await assetsQuery
//                .Include(a => a.Product)
//                .GroupBy(a => a.Product.Name)
//                .Select(g => new ResumenActivoViewModel
//                {
//                    ProductName = g.Key,
//                    Total = g.Count(),
//                    Disponibles = g.Count(x => x.Status == AssetStatus.Available),
//                    Asignados = g.Count(x => x.Status == AssetStatus.InUse),
//                    EnMantenimiento = g.Count(x => x.Status == AssetStatus.Maintenance)
//                }).ToListAsync();

//            return View(model);
//        }



//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetCreateProduct(int id = 0)
//        {
//            ViewBag.Categories = await _inventoryService.GetCategories();

//            if (id == 0)
//            {
//                ViewBag.SubCategories = new List<ProductSubcategory>();
//                return PartialView("Partials/_CreateProduct", new Product());
//            }

//            var product = await _context.Productos.Include(p => p.SubCategory).FirstOrDefaultAsync(p => p.Id == id);
//            if (product == null) return NotFound();

//            ViewBag.SubCategories = await _inventoryService.GetSubcategoriesByCategory(product.CategoryId);
//            return PartialView("Partials/_CreateProduct", product);
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetCreateCategory(int id = 0)
//        {
//            if (id == 0) return PartialView("Partials/_CreateCategory", new ProductCategory());
//            return PartialView("Partials/_CreateCategory", await _context.ProductosCategorias.FindAsync(id));
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetCreateSubcategory(int id = 0)
//        {
//            ViewBag.Categories = await _inventoryService.GetCategories();
//            if (id == 0) return PartialView("Partials/_CreateSubcategory", new ProductSubcategory());
//            return PartialView("Partials/_CreateSubcategory", await _context.ProductosSubcategorias.FindAsync(id));
//        }

//        public async Task<IActionResult> GetStockView(int branchId)
//        {
//            var stock = await _context.ProductosStock.Include(s => s.Product)
//                .Where(s => s.BranchId == branchId).ToListAsync();
//            return PartialView("Partials/_StockView", stock);
//        }

//        public async Task<IActionResult> GetAssetsView(int branchId)
//        {
//            var assets = await _context.ActivosFijos.Include(a => a.Product)
//                .Where(a => a.BranchId == branchId).ToListAsync();
//            return PartialView("Partials/_AssetsView", assets);
//        }

//        public async Task<IActionResult> GetPendingView(int branchId)
//        {
//            var pending = await _context.ProductosTransferencias
//                .Include(t => t.OriginBranch).Include(t => t.Details).ThenInclude(d => d.Product)
//                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent).ToListAsync();
//            return PartialView("Partials/_PendingView", pending);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetKardexView(int branchId)
//        {
//            var movements = await _context.MovimientosInventario
//                .Include(m => m.Product).Include(m => m.FixedAsset)
//                .Where(m => m.BranchId == branchId)
//                .OrderByDescending(m => m.CreatedAt).Take(100).ToListAsync();

//            return PartialView("Partials/_KardexTable", movements);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetStockEntry()
//        {
//            ViewBag.NextInternalCode = await _inventoryService.GenerateInternalCode();
//            var model = new StockEntryFormViewModel
//            {
//                Branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync()
//            };
//            return PartialView("Partials/_StockEntry", model);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetProductTransfer()
//        {
//            var year = DateTime.Now.Year;
//            var count = await _context.ProductosTransferencias.CountAsync(t => t.SentAt.Year == year);
//            ViewBag.NextTransferCode = $"TR-{year}-{(count + 1):D5}";

//            var model = new TransferFormViewModel
//            {
//                AllBranches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync(),
//                IsAdmin = User.IsInRole("Admin"),
//                UserBranchId = 1, // Ajustar segun Claim del usuario
//                UserBranchName = "Sede Central"
//            };
//            return PartialView("Partials/_ProductTransfer", model);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetMinStockSettings(int branchId)
//        {
//            var stocks = await _context.ProductosStock.Include(ps => ps.Product)
//                .Where(ps => ps.BranchId == branchId).ToListAsync();

//            ViewBag.BranchName = (await _context.Filiales.FindAsync(branchId))?.Name;
//            return PartialView("Partials/_SetMinimumStock", stocks);
//        }




//        // API para que el JS obtenga los productos (usada en PreloadProductData)
//        [HttpGet]
//        public async Task<IActionResult> GetProductData()
//        {
//            var products = await _inventoryService.GetProductCatalog();

//            var stockProds = products.Select(p => new {
//                id = p.Id,
//                name = p.Name,
//                unit = (int)p.Unit
//            });

//            return Json(new { stock = stockProds });
//        }

//        // API para obtener qué hay disponible en la sede seleccionada (usada en Transferencias)
//        [HttpGet]   
//        public async Task<IActionResult> GetAvailableItemsByBranch(int branchId)
//        {
//            // 1. Stock Consumible
//            var stock = await _context.ProductosStock
//                .Include(s => s.Product)
//                .Where(s => s.BranchId == branchId && s.Quantity > 0)
//                .Select(s => new {
//                    productId = s.ProductId,
//                    name = s.Product.Name,
//                    quantity = s.Quantity,
//                    unit = (int)s.Product.Unit
//                }).ToListAsync();

//            // 2. Activos Disponibles
//            var assets = await _context.ActivosFijos
//                .Include(a => a.Product)
//                .Where(a => a.BranchId == branchId && a.Status == AssetStatus.Available)
//                .Select(a => new {
//                    id = a.Id,
//                    productId = a.ProductId,
//                    productName = a.Product.Name,
//                    serialNumber = a.SerialNumber
//                }).ToListAsync();

//            return Json(new { stock, assets });
//        }

//        // API para obtener categorías dinámicamente
//        [HttpGet]
//        public async Task<IActionResult> GetCategories()
//            => Json(await _inventoryService.GetCategories());

//        // API para obtener subcategorías por categoría
//        [HttpGet]
//        public async Task<IActionResult> GetSubcategoriesByCategory(int categoryId)
//            => Json(await _inventoryService.GetSubcategoriesByCategory(categoryId));

//        // API para obtener el siguiente código patrimonial
//        [HttpGet]
//        public async Task<IActionResult> GetNextAssetCode(int productId)
//            => Json(new { code = await _inventoryService.GeneratePatrimonialCode(productId) });





//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> SaveProduct(Product model)
//        {
//            ModelState.Remove("Category"); ModelState.Remove("SubCategory");
//            if (!ModelState.IsValid) return Json(new { success = false, message = "Error de validación" });
//            var result = await _inventoryService.SaveProduct(model);
//            return Json(new { success = result.success, message = result.message + (result.sku != null ? $". SKU: {result.sku}" : "") });
//        }

//        [HttpPost]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> DeleteProduct(int id)
//        {
//            var result = await _inventoryService.DeleteProduct(id);
//            return Json(new { success = result.success, message = result.message });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ProcessBulkEntry(BulkEntryViewModel model)
//        {
//            var result = await _inventoryService.ProcessBulkEntry(model, User.Identity.Name);
//            if (result.success)
//            {
//                await _notificationService.CreateAsync("Ingreso Procesado", $"Guía {model.InternalControlNumber} correcta.", "Inventario.Ver", model.BranchId, Url.Action("Inventory", new { branchId = model.BranchId }), "fa-file-circle-check");
//            }
//            return Json(new { success = result.success, message = result.message });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ProcessTransfer([FromBody] TransferEntryViewModel model)
//        {
//            var result = await _inventoryService.ProcessTransfer(model, User.Identity.Name);
//            if (result.success)
//            {
//                await _notificationService.CreateAsync("Transferencia Recibida", $"Mercadería en camino. Guía: {result.finalCode}", "Inventario.Traslados", model.TargetBranchId, Url.Action("Inventory", new { branchId = model.TargetBranchId }), "fa-truck-ramp-box");
//            }
//            return Json(new { success = result.success, message = result.message });
//        }

//        [HttpPost]
//        public async Task<IActionResult> CancelTransfer(int id, string reason)
//        {
//            var result = await _inventoryService.CancelTransfer(id, reason, User.Identity.Name);
//            return Json(new { success = result.success, message = result.message });
//        }

//        public async Task<IActionResult> ImprimirGuia(string controlNumber)
//        {
//            var movimientos = await _context.MovimientosInventario
//                .Include(m => m.Product).Include(m => m.Branch).Include(m => m.FixedAsset)
//                .Where(m => m.InternalControlNumber == controlNumber).ToListAsync();

//            if (!movimientos.Any()) return NotFound();
//            var primerMov = movimientos.First();

//            var model = new ReporteGuiaViewModel
//            {
//                NumeroGuia = controlNumber,
//                Fecha = primerMov.CreatedAt,
//                UsuarioResponsable = primerMov.UserId,
//                DocumentoExterno = primerMov.ExternalDocumentNumber ?? "S/N",
//                Items = movimientos.Select(m => new DetalleGuiaItem
//                {
//                    Producto = m.Product?.Name,
//                    Cantidad = m.Quantity,
//                    Tipo = m.MovementType == MovementType.Entry ? "ENTRADA" : "SALIDA",
//                    Sede = m.Branch?.Name,
//                    CodigoPatrimonial = m.FixedAsset?.PatrimonialCode ?? "---"
//                }).ToList()
//            };

//            if (primerMov.Concept == Concept.Buy)
//            {
//                model.TipoOperacion = "ACTA DE RECEPCIÓN E INTERNAMIENTO";
//                model.SedeOrigen = "PROVEEDOR / COMPRA";
//                model.SedeDestino = primerMov.Branch?.Name;
//            }
//            else if (primerMov.Concept == Concept.Transfer)
//            {
//                model.TipoOperacion = "GUÍA DE REMISIÓN INTERNA (TRANSFERENCIA)";
//                model.SedeOrigen = movimientos.FirstOrDefault(m => m.MovementType == MovementType.Exit)?.Branch?.Name;
//                var transferencia = await _context.ProductosTransferencias.Include(t => t.TargetBranch).FirstOrDefaultAsync(t => t.InternalControlNumber == controlNumber);
//                model.SedeDestino = transferencia?.TargetBranch?.Name;
//            }

//            return new ViewAsPdf("GuideDocumentPDF", model)
//            {
//                PageSize = Rotativa.AspNetCore.Options.Size.A4,
//                CustomSwitches = "--page-offset 0 --footer-center [page]/[toPage] --footer-font-size 8"
//            };
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> SaveCategory(ProductCategory model)
//        {
//            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });

//            var result = await _inventoryService.SaveCategory(model);
//            return Json(new { success = result.success, message = result.message });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> SaveSubcategory(ProductSubcategory model)
//        {
//            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });

//            var result = await _inventoryService.SaveSubcategory(model);
//            return Json(new { success = result.success, message = result.message });
//        }

//        [HttpPost]
//        public async Task<IActionResult> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model)
//        {
//            var result = await _inventoryService.UpdateMinimumStocks(model);

//            if (result.success)
//                return Json(new { success = true, message = result.message });

//            return Json(new { success = false, message = result.message });
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ConfirmTransferReceipt(int transferId, string receptionObservation)
//        {
//            // Llamamos al servicio pasando los parámetros necesarios
//            // User.Identity.Name es el userId que pide tu lógica
//            var result = await _inventoryService.ConfirmTransferReceipt(transferId, receptionObservation, User.Identity.Name);

//            if (result.success)
//            {
//                // Opcional: Notificar al origen que la mercadería llegó
//                // Esto lo puedes hacer aquí o dentro del servicio si prefieres
//                return Json(new { success = true, message = "Mercadería recibida y stock actualizado correctamente." });
//            }

//            return Json(new { success = false, message = "Error al procesar la recepción: " + result.message });
//        }

//    }
//}

using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ContratosYReembolsos.Models.ViewModels.Inventory;

namespace ContratosYReembolsos.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;

        public InventoryController(ApplicationDbContext context, IInventoryService inventoryService, INotificationService notificationService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _notificationService = notificationService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index() => View(await _inventoryService.GetAdminDashboardSummary());

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Catalog()
        {
            ViewBag.Categories = await _inventoryService.GetCategories();
            ViewBag.SubCategories = await _context.ProductosSubcategorias.Include(s => s.Category).ToListAsync();
            return View(await _inventoryService.GetProductCatalog());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Categories() => View(await _inventoryService.GetCategories());

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Subcategories()
        {
            var subs = await _context.ProductosSubcategorias.Include(s => s.Category).ToListAsync();
            return View(subs);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BranchSelection()
        {
            var branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            return View(branches);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Inventory(int branchId)
        {
            var sede = await _context.Filiales.FindAsync(branchId);
            if (sede == null) return NotFound();

            // Cargamos stock sin filtros de ControlType
            var stockInventario = await _context.ProductosStock
                .Include(s => s.Product).ThenInclude(p => p.SubCategory)
                .Where(s => s.BranchId == branchId)
                .ToListAsync();

            var transferenciasPendientes = await _context.ProductosTransferencias
                .Include(t => t.OriginBranch)
                .Include(t => t.Details).ThenInclude(d => d.Product)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
                .ToListAsync();

            ViewBag.SedeNombre = sede.Name;
            ViewBag.BranchId = branchId;
            ViewBag.TransferenciasPendientes = transferenciasPendientes;
            ViewBag.TotalPendientes = transferenciasPendientes.Count;

            return View(stockInventario);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockView(int branchId)
        {
            var stock = await _context.ProductosStock
                .Include(s => s.Product)
                .Where(s => s.BranchId == branchId)
                .ToListAsync();
            return PartialView("Partials/_StockView", stock);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingView(int branchId)
        {
            var pending = await _context.ProductosTransferencias
                .Include(t => t.OriginBranch)
                .Include(t => t.Details).ThenInclude(d => d.Product)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
                .ToListAsync();
            return PartialView("Partials/_PendingView", pending);
        }

        [HttpGet]
        public async Task<IActionResult> GetKardexView(int branchId)
        {
            var movements = await _context.MovimientosInventario
                .Include(m => m.Product).Include(m => m.FixedAsset)
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.CreatedAt).Take(100).ToListAsync();

            return PartialView("Partials/_KardexTable", movements);
        }

        // --- APIs PARA JAVASCRIPT (MODALES) ---

        [HttpGet]
        public async Task<IActionResult> GetProductData()
        {
            // Limpieza total: Eliminado ControlType y la propiedad 'assets' del JSON
            var products = await _inventoryService.GetProductCatalog();
            var stockProds = products.Select(p => new {
                id = p.Id,
                name = p.Name,
                unit = (int)p.Unit
            });

            return Json(new { stock = stockProds });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableItemsByBranch(int branchId)
        {
            // Solo devolvemos stock consumible
            var stock = await _context.ProductosStock
                .Include(s => s.Product)
                .Where(s => s.BranchId == branchId && s.Quantity > 0)
                .Select(s => new {
                    productId = s.ProductId,
                    name = s.Product.Name,
                    quantity = s.Quantity,
                    unit = (int)s.Product.Unit
                }).ToListAsync();

            return Json(new { stock });
        }

        [HttpGet]
        public async Task<IActionResult> GetStockEntry()
        {
            ViewBag.NextInternalCode = await _inventoryService.GenerateInternalCode();
            var model = new StockEntryFormViewModel
            {
                Branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync()
            };
            return PartialView("Partials/_StockEntry", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductTransfer()
        {
            var year = DateTime.Now.Year;
            var count = await _context.ProductosTransferencias.CountAsync(t => t.SentAt.Year == year);
            ViewBag.NextTransferCode = $"TR-{year}-{(count + 1):D5}";

            var model = new TransferFormViewModel
            {
                AllBranches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync(),
                IsAdmin = User.IsInRole("Admin"),
                UserBranchId = 1, // Sincronizar con el usuario real en producción
                UserBranchName = "Sede Central"
            };
            return PartialView("Partials/_ProductTransfer", model);
        }

        // --- PROCESAMIENTO DE DATOS ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBulkEntry(BulkEntryViewModel model)
        {
            var result = await _inventoryService.ProcessBulkEntry(model, User.Identity.Name);
            if (result.success)
            {
                await _notificationService.CreateAsync("Ingreso Procesado", $"Guía {model.InternalControlNumber} correcta.", "Inventario.Ver", model.BranchId, Url.Action("Inventory", new { branchId = model.BranchId }), "fa-file-circle-check");
            }
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransfer([FromBody] TransferEntryViewModel model)
        {
            var result = await _inventoryService.ProcessTransfer(model, User.Identity.Name);
            if (result.success)
            {
                await _notificationService.CreateAsync("Transferencia Recibida", $"Mercadería en camino. Guía: {result.finalCode}", "Inventario.Traslados", model.TargetBranchId, Url.Action("Inventory", new { branchId = model.TargetBranchId }), "fa-truck-ramp-box");
            }
            return Json(new { success = result.success, message = result.message });
        }

        // --- OTROS MÉTODOS ---

        public async Task<IActionResult> Movements()
        {
            var movements = await _context.MovimientosInventario
                .Include(m => m.Product).Include(m => m.Branch)
                .OrderByDescending(m => m.CreatedAt).ToListAsync();
            return View(movements);
        }

        [HttpGet]
        public async Task<IActionResult> Kardex(int? branchId, int? productId, DateTime? start, DateTime? end)
        {
            ViewBag.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            ViewBag.Productos = await _context.Productos.OrderBy(p => p.Name).ToListAsync();

            var query = _context.MovimientosInventario
                .Include(m => m.Branch).Include(m => m.Product)
                .AsQueryable();

            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);
            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
            if (start.HasValue) query = query.Where(m => m.CreatedAt.Date >= start.Value.Date);
            if (end.HasValue) query = query.Where(m => m.CreatedAt.Date <= end.Value.Date);

            return View(await query.OrderByDescending(m => m.CreatedAt).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> InventoryDashboard(int? branchId, int? productId, string period = "monthly", string date = null)
        {
            var model = new InventoryDashboardViewModel { SelectedBranchId = branchId, SelectedProductId = productId, SelectedPeriod = period ?? "monthly" };
            DateTime refDate = DateTime.Now;
            DateTime startDate, endDate;

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

            List<string> labels = new List<string>();
            switch (model.SelectedPeriod.ToLower())
            {
                case "daily": startDate = refDate.Date; endDate = startDate.AddDays(1).AddTicks(-1); labels = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToList(); break;
                case "weekly": int diff = (7 + (refDate.DayOfWeek - DayOfWeek.Monday)) % 7; startDate = refDate.AddDays(-1 * diff).Date; endDate = startDate.AddDays(7).AddTicks(-1); labels = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" }; break;
                case "yearly": startDate = new DateTime(refDate.Year, 1, 1); endDate = startDate.AddYears(1).AddDays(-1); labels = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).Select(m => m.ToUpper()).ToList(); break;
                default: startDate = new DateTime(refDate.Year, refDate.Month, 1); endDate = startDate.AddMonths(1).AddTicks(-1); labels = Enumerable.Range(1, DateTime.DaysInMonth(refDate.Year, refDate.Month)).Select(d => $"Día {d}").ToList(); break;
            }

            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            var productosQuery = _context.Productos.AsQueryable();
            if (branchId.HasValue)
            {
                var idsEnSede = await _context.ProductosStock.Where(s => s.BranchId == branchId).Select(s => s.ProductId).Distinct().ToListAsync();
                productosQuery = productosQuery.Where(p => idsEnSede.Contains(p.Id));
            }
            model.Productos = await productosQuery.OrderBy(p => p.Name).ToListAsync();

            var movimientos = await _context.MovimientosInventario.Include(m => m.Product)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .Where(m => !branchId.HasValue || m.BranchId == branchId.Value)
                .Where(m => !productId.HasValue || m.ProductId == productId.Value)
                .ToListAsync();

            List<decimal> valoresConsumo = new List<decimal>();
            List<decimal> valoresIngreso = new List<decimal>();

            for (int i = 0; i < labels.Count; i++)
            {
                var temp = movimientos.Where(m => (period == "daily" && m.CreatedAt.Hour == i) || (period == "weekly" && ((int)m.CreatedAt.DayOfWeek + 6) % 7 == i) || (period == "yearly" && m.CreatedAt.Month == (i + 1)) || (period == "monthly" && m.CreatedAt.Day == (i + 1)));
                valoresConsumo.Add(temp.Where(m => m.MovementType == MovementType.Exit).Sum(x => x.Quantity));
                valoresIngreso.Add(temp.Where(m => m.MovementType == MovementType.Entry).Sum(x => x.Quantity));
            }

            model.GraficoLabelsJson = JsonConvert.SerializeObject(labels);
            model.ValoresConsumoJson = JsonConvert.SerializeObject(valoresConsumo);
            model.ValoresIngresoJson = JsonConvert.SerializeObject(valoresIngreso);
            model.TotalConsumo = valoresConsumo.Sum();
            model.TotalIngreso = valoresIngreso.Sum();

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

            return View(model);
        }

        public async Task<IActionResult> ImprimirGuia(string controlNumber)
        {
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Product).Include(m => m.Branch)
                .Where(m => m.InternalControlNumber == controlNumber).ToListAsync();

            if (!movimientos.Any()) return NotFound();
            var primerMov = movimientos.First();

            var model = new ReporteGuiaViewModel
            {
                NumeroGuia = controlNumber,
                Fecha = primerMov.CreatedAt,
                UsuarioResponsable = primerMov.UserId,
                DocumentoExterno = primerMov.ExternalDocumentNumber ?? "S/N",
                Items = movimientos.Select(m => new DetalleGuiaItem { Producto = m.Product?.Name, Cantidad = m.Quantity, Tipo = m.MovementType == MovementType.Entry ? "ENTRADA" : "SALIDA", Sede = m.Branch?.Name, CodigoPatrimonial = "---" }).ToList()
            };

            if (primerMov.Concept == Concept.Buy) { model.TipoOperacion = "ACTA DE RECEPCIÓN E INTERNAMIENTO"; model.SedeOrigen = "PROVEEDOR / COMPRA"; model.SedeDestino = primerMov.Branch?.Name; }
            else if (primerMov.Concept == Concept.Transfer)
            {
                model.TipoOperacion = "GUÍA DE REMISIÓN INTERNA (TRANSFERENCIA)";
                model.SedeOrigen = movimientos.FirstOrDefault(m => m.MovementType == MovementType.Exit)?.Branch?.Name;
                var transferencia = await _context.ProductosTransferencias.Include(t => t.TargetBranch).FirstOrDefaultAsync(t => t.InternalControlNumber == controlNumber);
                model.SedeDestino = transferencia?.TargetBranch?.Name;
            }

            return new ViewAsPdf("GuideDocumentPDF", model) { PageSize = Rotativa.AspNetCore.Options.Size.A4, CustomSwitches = "--page-offset 0 --footer-center [page]/[toPage] --footer-font-size 8" };
        }

        // --- MÉTODOS DE SERVICIO RESTANTES ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(ProductCategory model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });
            var result = await _inventoryService.SaveCategory(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubcategory(ProductSubcategory model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });
            var result = await _inventoryService.SaveSubcategory(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model)
        {
            var result = await _inventoryService.UpdateMinimumStocks(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransferReceipt(int transferId, string receptionObservation)
        {
            var result = await _inventoryService.ConfirmTransferReceipt(transferId, receptionObservation, User.Identity.Name);
            return Json(new { success = result.success, message = result.message });
        }
    }
}