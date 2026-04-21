using System.Globalization;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.ValueObjects;
using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Rotativa.AspNetCore;

namespace ContratosYReembolsos.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly INotificationService _notificationService;

        public InventoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var summary = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Productos.CountAsync(),
                TotalAssets = await _context.ActivosFijos.CountAsync(),
                PendingTransfers = await _context.ProductosTransferencias.CountAsync(t => t.Status == TransferStatus.Sent),
                LowStockItems = await _context.ProductosStock.CountAsync(s => s.Quantity <= 5),
                Branches = await _context.Filiales.ToListAsync()
            };

            return View(summary);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Catalog()
        {
            var products = await _context.Productos
                .Include(p => p.SubCategory)
                    .ThenInclude(s => s.Category)
                .ToListAsync();

            // Listas para los selectores (DropDowns)
            ViewBag.Categories = await _context.ProductosCategorias.ToListAsync();
            ViewBag.SubCategories = await _context.ProductosSubcategorias
                .Include(s => s.Category)
                .ToListAsync();

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCreateProduct(int id = 0)
        {
            ViewBag.Categories = await _context.ProductosCategorias.ToListAsync();

            if (id == 0)
            {
                ViewBag.SubCategories = new List<ProductSubcategory>();
                return PartialView("Partials/_CreateProduct", new Product());
            }

            var product = await _context.Productos.Include(p => p.SubCategory).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            // Cargar solo las subcategorías de la categoría que ya tiene el producto
            ViewBag.SubCategories = await _context.ProductosSubcategorias
                .Where(s => s.CategoryId == product.CategoryId)
                .ToListAsync();

            return PartialView("Partials/_CreateProduct", product);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.ProductosCategorias
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategoriesByCategory(int categoryId)
        {
            var subcategories = await _context.ProductosSubcategorias
                .Where(s => s.CategoryId == categoryId)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Json(subcategories);
        }

        // POST: Guardar los datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveProduct(Product model)
        {
            ModelState.Remove("Category");
            ModelState.Remove("SubCategory");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Validación fallida: " + errors });
            }

            try
            {
                var subCategory = await _context.ProductosSubcategorias
                    .Include(sc => sc.Category)
                    .FirstOrDefaultAsync(sc => sc.Id == model.SubCategoryId);

                if (subCategory == null) return Json(new { success = false, message = "Subcategoría inválida." });

                model.CategoryId = subCategory.CategoryId;

                if (model.Id == 0)
                {
                    // --- LÓGICA DE GENERACIÓN DE SKU ---

                    // 1. Obtener prefijos (3 letras)
                    string catPrefix = (subCategory.Category.Name.Length >= 3
                        ? subCategory.Category.Name.Substring(0, 3)
                        : subCategory.Category.Name).ToUpper();

                    string subPrefix = (subCategory.Name.Length >= 3
                        ? subCategory.Name.Substring(0, 3)
                        : subCategory.Name).ToUpper();

                    // 2. Obtener el correlativo de 6 dígitos
                    // Contamos cuántos productos existen ya en esta combinación de categoría y subcategoría
                    int nextCount = await _context.Productos
                        .CountAsync(p => p.SubCategoryId == model.SubCategoryId) + 1;

                    string correlativo = nextCount.ToString("D6"); // "D6" rellena con ceros a la izquierda hasta 6 dígitos

                    // 3. Ensamblar: CAT[ID]-SUB[ID]-000000
                    model.Sku = $"{catPrefix}{model.CategoryId}-{subPrefix}{model.SubCategoryId}-{correlativo}";

                    _context.Productos.Add(model);
                }
                else
                {
                    _context.Update(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Producto guardado. SKU generado: {model.Sku}" });
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Error de base de datos: " + innerError });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Productos.FindAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "El producto no existe." });
                }

                bool hasStock = await _context.ProductosStock
                    .AnyAsync(s => s.ProductId == id && s.Quantity > 0);

                if (hasStock)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se puede eliminar: El producto tiene existencias registradas en el inventario de las sedes."
                    });
                }

                _context.Productos.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Producto eliminado correctamente." });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "No se puede eliminar porque el producto ya tiene historial de movimientos (Kardex) vinculado."
                });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ProductosCategorias.ToListAsync();
            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Subcategories()
        {
            var subs = await _context.ProductosSubcategorias
                .Include(s => s.Category)
                .ToListAsync();
            return View(subs);
        }

        // --- GESTIÓN DE CATEGORÍAS ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCreateCategory(int id = 0)
        {
            if (id == 0) return PartialView("Partials/_CreateCategory", new ProductCategory());
            var category = await _context.ProductosCategorias.FindAsync(id);
            return PartialView("Partials/_CreateCategory", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(ProductCategory model)
        {
            if (!ModelState.IsValid) return BadRequest("Datos inválidos");
            if (model.Id == 0) _context.ProductosCategorias.Add(model);
            else _context.Entry(model).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Categoría guardada" });
        }

        // --- GESTIÓN DE SUBCATEGORÍAS ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCreateSubcategory(int id = 0)
        {
            ViewBag.Categories = await _context.ProductosCategorias.ToListAsync();

            if (id == 0) return PartialView("Partials/_CreateSubcategory", new ProductSubcategory());
            var sub = await _context.ProductosSubcategorias.FindAsync(id);
            return PartialView("Partials/_CreateSubcategory", sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubcategory(ProductSubcategory model)
        {
            if (!ModelState.IsValid) return BadRequest("Datos inválidos");
            if (model.Id == 0) _context.ProductosSubcategorias.Add(model);
            else _context.Entry(model).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Subcategoría guardada" });
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

            // 1. Obtener Stock de Inventario (Suma de cantidades)
            var stockInventario = await _context.ProductosStock
                .Include(s => s.Product)
                .ThenInclude(p => p.SubCategory)
                .Where(s => s.BranchId == branchId && s.Product.ControlType == ControlType.Stock)
                .ToListAsync();

            // 2. Obtener Activos Fijos (Detalle individual)
            var activosFijos = await _context.ActivosFijos
                .Include(a => a.Product)
                .Where(a => a.BranchId == branchId)
                .ToListAsync();

            ViewBag.SedeNombre = sede.Name;
            ViewBag.BranchId = branchId;

            ViewBag.TransferenciasPendientes = await _context.ProductosTransferencias
                .Include(t => t.OriginBranch)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Product)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
                .ToListAsync();

            ViewBag.TotalPendientes = ((List<ProductTransfer>)ViewBag.TransferenciasPendientes).Count;

            return View((stockInventario, activosFijos));
        }

        public async Task<IActionResult> GetStockView(int branchId)
        {
            var stock = await _context.ProductosStock.Include(s => s.Product)
                .Where(s => s.BranchId == branchId).ToListAsync();
            return PartialView("Partials/_StockView", stock);
        }

        public async Task<IActionResult> GetAssetsView(int branchId)
        {
            var assets = await _context.ActivosFijos.Include(a => a.Product)
                .Where(a => a.BranchId == branchId).ToListAsync();
            return PartialView("Partials/_AssetsView", assets);
        }

        public async Task<IActionResult> GetPendingView(int branchId)
        {
            var pending = await _context.ProductosTransferencias
                .Include(t => t.OriginBranch).Include(t => t.Details).ThenInclude(d => d.Product)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent).ToListAsync();
            return PartialView("Partials/_PendingView", pending);
        }

        [HttpGet]
        public async Task<IActionResult> GetKardexView(int branchId)
        {
            var movements = await _context.MovimientosInventario
                .Include(m => m.Product)
                .Include(m => m.FixedAsset)
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(100) // Limitamos a los últimos 100 para no sobrecargar el Tab
                .ToListAsync();

            return PartialView("Partials/_KardexTable", movements);
        }

        private async Task<string> GenerateInternalCode()
        {
            var year = DateTime.Now.Year;
            var count = await _context.MovimientosInventario
                .CountAsync(m => m.CreatedAt.Year == year && m.Concept == Concept.Buy);

            return $"NI-{year}-{(count + 1).ToString("D5")}";
        }

        [HttpGet]
        public async Task<IActionResult> GetStockEntry()
        {
            // 1. Generamos el código (NI-2026-0001)
            ViewBag.NextInternalCode = await GenerateInternalCode();

            // 2. Preparamos el ViewModel solo con las sedes
            var model = new StockEntryFormViewModel
            {
                Branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync()
            };

            return PartialView("Partials/_StockEntry", model);
        }

        // Nueva API para que el JS obtenga los productos una sola vez
        [HttpGet]
        public async Task<IActionResult> GetProductData()
        {
            // Incluimos 'unit' en el objeto anónimo para que llegue al JS
            var stockProds = await _context.Productos
                .Where(p => p.ControlType == ControlType.Stock)
                .Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    unit = (int)p.Unit // Convertimos el Enum a int (0, 1, 2...)
                }).ToListAsync();

            var assetProds = await _context.Productos
                .Where(p => p.ControlType == ControlType.Asset)
                .Select(p => new {
                    id = p.Id,
                    name = p.Name,
                    unit = (int)p.Unit
                }).ToListAsync();

            return Json(new { stock = stockProds, assets = assetProds });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBulkEntry(BulkEntryViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
                return Json(new { success = false, message = "No hay ítems para ingresar." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string currentUserId = User.Identity?.Name ?? "Sistema";
                int currentYear = DateTime.Now.Year;

                foreach (var item in model.Items)
                {
                    int finalProductId = item.IsAsset ? item.ProductIdAsset.Value : item.ProductId;

                    if (item.IsAsset)
                    {
                        // --- CASO ACTIVO FIJO ---
                        var product = await _context.Productos
                            .Include(p => p.SubCategory)
                            .ThenInclude(sc => sc.Category)
                            .FirstOrDefaultAsync(p => p.Id == finalProductId);

                        if (product == null) throw new Exception($"Producto ID {finalProductId} no encontrado.");

                        string catPart = $"{(product.SubCategory.Category.Name.Length >= 3 ? product.SubCategory.Category.Name.Substring(0, 3) : product.SubCategory.Category.Name).ToUpper()}{product.SubCategory.Category.Id}";
                        string subPart = $"{(product.SubCategory.Name.Length >= 3 ? product.SubCategory.Name.Substring(0, 3) : product.SubCategory.Name).ToUpper()}{product.SubCategory.Id}";
                        string prefixFilter = $"FON-{currentYear}-{catPart}-{subPart}-";

                        var lastAsset = await _context.ActivosFijos
                            .Where(a => a.PatrimonialCode.StartsWith(prefixFilter))
                            .OrderByDescending(a => a.PatrimonialCode)
                            .FirstOrDefaultAsync();

                        int nextNumber = 1;
                        if (lastAsset != null)
                        {
                            string lastPart = lastAsset.PatrimonialCode.Split('-').Last();
                            if (int.TryParse(lastPart, out int lastNum)) nextNumber = lastNum + 1;
                        }

                        for (int i = 0; i < item.Quantity; i++)
                        {
                            string newPatrimonialCode = $"{prefixFilter}{nextNumber:D5}";
                            var newAsset = new FixedAsset
                            {
                                ProductId = finalProductId,
                                BranchId = model.BranchId,
                                SerialNumber = item.SerialNumber ?? "S/N",
                                PatrimonialCode = newPatrimonialCode,
                                Status = "Available",
                                CreatedAt = DateTime.Now
                            };

                            _context.ActivosFijos.Add(newAsset);
                            await _context.SaveChangesAsync();

                            _context.MovimientosInventario.Add(new InventoryMovement
                            {
                                ProductId = finalProductId,
                                BranchId = model.BranchId,
                                FixedAssetId = newAsset.Id,
                                Quantity = 1,
                                PreviousQuantity = 0,
                                NewQuantity = 1,
                                Concept = Concept.Buy,
                                MovementType = MovementType.Entry,
                                InternalControlNumber = model.InternalControlNumber,
                                ExternalDocumentNumber = model.ExternalDocumentNumber,
                                Description = $"{model.Description} (Cód: {newPatrimonialCode})",
                                UserId = currentUserId,
                                CreatedAt = DateTime.Now
                            });

                            nextNumber++;
                        }
                    }
                    else
                    {
                        // --- CASO STOCK CONSUMIBLE ---
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == finalProductId);

                        decimal cantAnterior = stock?.Quantity ?? 0;
                        decimal cantNueva = cantAnterior + item.Quantity;

                        if (stock == null)
                        {
                            stock = new ProductStock { BranchId = model.BranchId, ProductId = finalProductId, Quantity = item.Quantity };
                            _context.ProductosStock.Add(stock);
                        }
                        else
                        {
                            stock.Quantity = cantNueva;
                        }

                        await _context.SaveChangesAsync();

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = finalProductId,
                            BranchId = model.BranchId,
                            ProductStockId = stock.Id,
                            Quantity = item.Quantity,
                            PreviousQuantity = cantAnterior,
                            NewQuantity = cantNueva,
                            Concept = Concept.Buy,
                            MovementType = MovementType.Entry,
                            InternalControlNumber = model.InternalControlNumber,
                            ExternalDocumentNumber = model.ExternalDocumentNumber,
                            Description = model.Description,
                            UserId = currentUserId,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ============================================================
                // SISTEMA DE NOTIFICACIONES DINÁMICO
                // ============================================================

                // 1. Notificación General de Éxito (Sin GroupingKey para que siempre llegue)
                string inventoryUrl = Url.Action("Inventory", "Inventory", new { branchId = model.BranchId });

                await _notificationService.CreateAsync(
                    "Ingreso Procesado",
                    $"Se registró el ingreso masivo {model.InternalControlNumber} correctamente.",
                    "Inventario.Ver",
                    model.BranchId,
                    inventoryUrl,
                    "fa-file-circle-check"
                );

                // 2. Validación de Stock Crítico Post-Ingreso
                var idsIngresados = model.Items.Where(i => !i.IsAsset).Select(i => i.ProductId).ToList();

                var alertasStock = await _context.ProductosStock
                    .Include(ps => ps.Product)
                    .Where(ps => ps.BranchId == model.BranchId &&
                                 idsIngresados.Contains(ps.ProductId) &&
                                 ps.Quantity <= ps.MinimumStock)
                    .ToListAsync();

                foreach (var alerta in alertasStock)
                {
                    // Generamos la Clave Única para evitar duplicados en el F5 del GetLatest
                    string gKey = $"LOW_STOCK_{alerta.ProductId}_{model.BranchId}";

                    await _notificationService.CreateAsync(
                        "¡Atención: Stock Bajo!",
                        $"{alerta.Product.Name} sigue bajo el mínimo tras el ingreso ({alerta.Quantity} unidades).",
                        "Inventario.Ver",
                        model.BranchId,
                        inventoryUrl,
                        "fa-triangle-exclamation",
                        gKey // <--- Aquí la magia del GroupingKey
                    );
                }

                return Json(new { success = true, message = $"Ingreso {model.InternalControlNumber} procesado correctamente." });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return Json(new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNextAssetCode(int productId)
        {
            if (productId <= 0) return Json(new { code = "SELECCIONE PRODUCTO" });
            string nextCode = await GeneratePatrimonialCode(productId);
            return Json(new { code = nextCode });
        }

        private async Task<string> GeneratePatrimonialCode(int productId)
        {
            var product = await _context.Productos
                .Include(p => p.SubCategory)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product?.SubCategory?.Category == null) return "FONGEN0000001";

            string catName = product.SubCategory.Category.Name ?? "GEN";
            string catPrefix = catName.Length >= 3 ? catName.Substring(0, 3).ToUpper() : catName.ToUpper();
            int catId = product.SubCategory.CategoryId;
            string year = DateTime.Now.Year.ToString();

            string prefix = $"FON{catPrefix}{catId}{year}";

            // Buscar el último correlativo en la BD
            var lastCode = await _context.ActivosFijos
                .Where(a => a.PatrimonialCode.StartsWith(prefix))
                .OrderByDescending(a => a.PatrimonialCode)
                .Select(a => a.PatrimonialCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastCode != null)
            {
                string numberPart = lastCode.Replace(prefix, "");
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}"; // 5 dígitos: 00001
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEntry(int productId, int branchId, int quantity, string? observation, string? serialNumber)
        {
            var product = await _context.Productos.FindAsync(productId);
            if (product == null) return Json(new { success = false, message = "Producto no encontrado" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string currentUserId = User.Identity?.Name ?? "Sistema";

                if (product.ControlType == ControlType.Stock)
                {
                    var stock = await _context.ProductosStock
                        .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

                    if (stock == null)
                    {
                        stock = new ProductStock { ProductId = productId, BranchId = branchId, Quantity = quantity };
                        _context.ProductosStock.Add(stock);
                    }
                    else
                    {
                        stock.Quantity += quantity;
                    }

                    await _context.SaveChangesAsync(); // Para obtener stock.Id

                    _context.MovimientosInventario.Add(new InventoryMovement
                    {
                        ProductId = productId,
                        BranchId = branchId,
                        ProductStockId = stock.Id, // Vinculación exacta
                        Quantity = quantity,
                        MovementType = MovementType.Entry,
                        Description = observation ?? "Ingreso de mercadería",
                        CreatedAt = DateTime.Now,
                        UserId = currentUserId
                    });
                }
                else if (product.ControlType == ControlType.Asset)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        string realCode = await GeneratePatrimonialCode(productId);

                        var newAsset = new FixedAsset
                        {
                            ProductId = productId,
                            BranchId = branchId,
                            SerialNumber = serialNumber ?? "S/N",
                            PatrimonialCode = realCode,
                            Status = AssetStatus.Available.ToString()
                        };
                        _context.ActivosFijos.Add(newAsset);
                        await _context.SaveChangesAsync(); // Para obtener newAsset.Id

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = productId,
                            BranchId = branchId,
                            FixedAssetId = newAsset.Id, // Vinculación exacta al activo individual
                            Quantity = 1,
                            MovementType = MovementType.Entry,
                            Description = $"Alta de Activo | {observation}",
                            CreatedAt = DateTime.Now,
                            UserId = currentUserId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Ingreso procesado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Cargar estructura del modal
        [HttpGet]
        public async Task<IActionResult> GetProductTransfer()
        {
            // Lógica de generación de código TR-2026-00001
            var year = DateTime.Now.Year;
            var count = await _context.ProductosTransferencias.CountAsync(t => t.SentAt.Year == year);
            ViewBag.NextTransferCode = $"TR-{year}-{(count + 1).ToString("D5")}";

            var branches = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();

            // Asumiendo que obtienes los datos del usuario logueado
            var model = new TransferFormViewModel
            {
                AllBranches = branches,
                IsAdmin = User.IsInRole("Admin"),
                UserBranchId = 1, // ID Real
                UserBranchName = "Sede Central" // Nombre Real
            };

            return PartialView("Partials/_ProductTransfer", model);
        }

        // API para obtener qué hay disponible en la sede seleccionada
        [HttpGet]
        public async Task<IActionResult> GetAvailableItemsByBranch(int branchId)
        {
            // 1. Stock Consumible
            var stock = await _context.ProductosStock
                .Include(s => s.Product)
                .Where(s => s.BranchId == branchId && s.Quantity > 0)
                .Select(s => new
                {
                    productId = s.ProductId,
                    name = s.Product.Name,
                    quantity = s.Quantity,
                    unit = (int)s.Product.Unit
                }).ToListAsync();

            // 2. Activos Disponibles
            var assets = await _context.ActivosFijos
                .Include(a => a.Product)
                .Where(a => a.BranchId == branchId && a.Status == "Available")
                .Select(a => new
                {
                    id = a.Id,
                    productId = a.ProductId,
                    productName = a.Product.Name,
                    serialNumber = a.SerialNumber
                }).ToListAsync();

            return Json(new { stock, assets });
        }

        // POST: Procesar la transferencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransfer([FromBody] TransferEntryViewModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Los datos de la transferencia no llegaron correctamente al servidor." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string currentUserId = User.Identity?.Name ?? "Sistema";
                var year = DateTime.Now.Year;

                var transfer = new ProductTransfer
                {
                    InternalControlNumber = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 8),
                    OriginBranchId = model.OriginBranchId,
                    TargetBranchId = model.TargetBranchId,
                    Status = TransferStatus.Sent,
                    Observation = model.Observation,
                    SentAt = DateTime.Now,
                    SentByUserId = currentUserId,
                    Details = new List<ProductTransferDetail>()
                };

                _context.ProductosTransferencias.Add(transfer);
                await _context.SaveChangesAsync();

                string finalCode = $"TR-{year}-{transfer.Id.ToString("D5")}";
                transfer.InternalControlNumber = finalCode;

                foreach (var item in model.Items)
                {
                    if (item.IsAsset)
                    {
                        var asset = await _context.ActivosFijos.FindAsync(item.FixedAssetId);
                        if (asset == null) throw new Exception("Activo no encontrado");

                        asset.Status = "InTransit";

                        transfer.Details.Add(new ProductTransferDetail
                        {
                            ProductId = item.ProductId,
                            FixedAssetId = item.FixedAssetId,
                            Quantity = 1
                        });

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = item.ProductId,
                            BranchId = model.OriginBranchId,
                            FixedAssetId = item.FixedAssetId,
                            Quantity = 1,
                            // Lógica de Saldos para Activos (siempre es 1 a 0 en la sede actual)
                            PreviousQuantity = 1,
                            NewQuantity = 0,
                            Concept = Concept.Transfer,
                            MovementType = MovementType.Exit,
                            InternalControlNumber = finalCode,
                            TransferId = transfer.Id,
                            Description = $"Salida por transferencia {finalCode}",
                            UserId = currentUserId
                        });
                    }
                    else
                    {
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == model.OriginBranchId && s.ProductId == item.ProductId);

                        if (stock == null || stock.Quantity < item.Quantity)
                            throw new Exception($"Stock insuficiente para el producto {item.ProductId}");

                        // --- CAPTURA DE SALDOS ---
                        decimal cantAnterior = stock.Quantity;
                        stock.Quantity -= item.Quantity;
                        decimal cantNueva = stock.Quantity;

                        transfer.Details.Add(new ProductTransferDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = item.ProductId,
                            BranchId = model.OriginBranchId,
                            ProductStockId = stock.Id,
                            Quantity = item.Quantity,
                            PreviousQuantity = cantAnterior, // <--- Dato nuevo
                            NewQuantity = cantNueva,         // <--- Dato nuevo
                            Concept = Concept.Transfer,
                            MovementType = MovementType.Exit,
                            InternalControlNumber = finalCode,
                            TransferId = transfer.Id,
                            Description = $"Salida por transferencia {finalCode}",
                            UserId = currentUserId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ============================================================
                // NOTIFICACIONES SIGNALR
                // ============================================================
                var originBranch = await _context.Filiales.FindAsync(model.OriginBranchId);

                // URL para que el destino vea sus transferencias pendientes
                string targetUrl = Url.Action("Inventory", "Inventory", new { branchId = model.TargetBranchId });

                await _notificationService.CreateAsync(
                    "Transferencia Recibida",
                    $"Nueva mercadería en camino desde {originBranch?.Name}. Guía: {finalCode}",
                    "Inventario.Traslados",
                    model.TargetBranchId,
                    targetUrl,
                    "fa-truck-ramp-box"
                );


                return Json(new { success = true, message = $"Transferencia {finalCode} iniciada." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Acción para confirmar la recepción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransferReceipt(int transferId, string receptionObservation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string currentUserId = User.Identity?.Name ?? "Sistema";

                var transfer = await _context.ProductosTransferencias
                    .Include(t => t.Details)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null || transfer.Status != TransferStatus.Sent)
                    return Json(new { success = false, message = "La transferencia no existe." });

                transfer.Status = TransferStatus.Received;
                transfer.ReceivedAt = DateTime.Now;
                transfer.ReceivedByUserId = currentUserId;
                transfer.ReceptionObservation = receptionObservation;

                foreach (var det in transfer.Details)
                {
                    if (det.FixedAssetId.HasValue)
                    {
                        var asset = await _context.ActivosFijos.FindAsync(det.FixedAssetId);
                        if (asset != null)
                        {
                            asset.BranchId = transfer.TargetBranchId;
                            asset.Status = "Available";
                        }

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = det.ProductId,
                            BranchId = transfer.TargetBranchId,
                            FixedAssetId = det.FixedAssetId,
                            Quantity = 1,
                            PreviousQuantity = 0, // En destino no había este activo específico
                            NewQuantity = 1,
                            Concept = Concept.Transfer,
                            MovementType = MovementType.Entry,
                            InternalControlNumber = transfer.InternalControlNumber,
                            TransferId = transfer.Id,
                            Description = $"Recepción de transferencia {transfer.InternalControlNumber}",
                            UserId = currentUserId
                        });
                    }
                    else
                    {
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == transfer.TargetBranchId && s.ProductId == det.ProductId);

                        // --- CAPTURA DE SALDOS ---
                        decimal cantAnterior = stock?.Quantity ?? 0;
                        decimal cantNueva = cantAnterior + det.Quantity;

                        if (stock == null)
                        {
                            stock = new ProductStock
                            {
                                BranchId = transfer.TargetBranchId,
                                ProductId = det.ProductId,
                                Quantity = det.Quantity
                            };
                            _context.ProductosStock.Add(stock);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            stock.Quantity = cantNueva;
                            await _context.SaveChangesAsync();
                        }

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = det.ProductId,
                            BranchId = transfer.TargetBranchId,
                            ProductStockId = stock.Id,
                            Quantity = det.Quantity,
                            PreviousQuantity = cantAnterior, // <--- Dato nuevo
                            NewQuantity = cantNueva,         // <--- Dato nuevo
                            Concept = Concept.Transfer,
                            MovementType = MovementType.Entry,
                            InternalControlNumber = transfer.InternalControlNumber,
                            TransferId = transfer.Id,
                            Description = $"Recepción de transferencia {transfer.InternalControlNumber}",
                            UserId = currentUserId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ============================================================
                // NOTIFICACIÓN DE RETORNO (Aviso al Origen)
                // ============================================================
                var targetBranch = await _context.Filiales.FindAsync(transfer.TargetBranchId);

                // Notificamos a la sede que envió que ya recibieron sus productos
                await _notificationService.CreateAsync(
                    "Transferencia Confirmada",
                    $"La sede {targetBranch?.Name} recibió la mercadería de la guía {transfer.InternalControlNumber}.",
                    "Inventario.Ver",
                    transfer.OriginBranchId, // Sede de origen
                    null, // No necesita redirección específica
                    "fa-circle-check"
                );


                return Json(new { success = true, message = "Mercadería recibida correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelTransfer(int id, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ProductosTransferencias
                    .Include(t => t.Details)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transfer == null || transfer.Status == TransferStatus.Received)
                    return Json(new { success = false, message = "No se puede anular una transferencia ya recibida." });

                // 1. Revertir Stock en Origen
                foreach (var det in transfer.Details)
                {
                    if (det.FixedAssetId.HasValue)
                    {
                        // Si es Activo Fijo, lo volvemos a poner como Disponible en la sede origen
                        var asset = await _context.ActivosFijos.FindAsync(det.FixedAssetId);
                        if (asset != null) asset.Status = "Disponible";
                    }
                    else
                    {
                        // Si es suministro, sumamos la cantidad de vuelta a la sede origen
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == transfer.OriginBranchId && s.ProductId == det.ProductId);
                        if (stock != null) stock.Quantity += det.Quantity;
                    }

                    // 2. Registrar movimiento de REVERSO en el Kardex (InventoryMovement)
                    var reverso = new InventoryMovement
                    {
                        ProductId = det.ProductId,
                        BranchId = transfer.OriginBranchId,
                        Quantity = det.Quantity,
                        Concept = Concept.Adjustment, // O crear uno llamado "Reverso"
                        MovementType = MovementType.Entry,
                        InternalControlNumber = transfer.InternalControlNumber,
                        Description = $"ANULACIÓN DE GUÍA: {reason}",
                        UserId = User.Identity.Name // O el ID del usuario actual
                    };
                    _context.MovimientosInventario.Add(reverso);
                }

                // 3. Actualizar estado de la transferencia
                transfer.Status = TransferStatus.Cancelled;
                transfer.ReceptionObservation = $"ANULADA: {reason}";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Transferencia anulada correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Kardex(int? branchId, int? productId, DateTime? start, DateTime? end)
        {
            // 1. Cargar catálogos para los filtros de la vista
            ViewBag.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            ViewBag.Productos = await _context.Productos.OrderBy(p => p.Name).ToListAsync();

            // 2. Base de la consulta con todos los Includes necesarios
            var query = _context.MovimientosInventario
                .Include(m => m.Branch)
                .Include(m => m.Product)
                .Include(m => m.FixedAsset) // Importante para mostrar las series
                .AsQueryable();

            // 3. Aplicación de filtros dinámicos
            if (branchId.HasValue)
            {
                query = query.Where(m => m.BranchId == branchId.Value);
            }

            if (productId.HasValue)
            {
                query = query.Where(m => m.ProductId == productId.Value);
            }

            if (start.HasValue)
            {
                // Usamos .Date para asegurar que tome desde el inicio del día
                query = query.Where(m => m.CreatedAt.Date >= start.Value.Date);
            }

            if (end.HasValue)
            {
                // Usamos .Date para asegurar que tome hasta el final del día
                query = query.Where(m => m.CreatedAt.Date <= end.Value.Date);
            }

            // 4. Ordenar por fecha (los más recientes primero)
            var results = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();

            // 5. Retornar a la vista
            return View(results);
        }

        // Métodos de Acción actualizados
        [HttpGet]
        public async Task<IActionResult> InventoryDashboard(int? branchId, int? productId, string period = "monthly", string date = null)
        {
            var model = new InventoryDashboardViewModel
            {
                SelectedBranchId = branchId,
                SelectedProductId = productId,
                SelectedPeriod = period ?? "monthly"
            };

            DateTime refDate = DateTime.Now;
            DateTime startDate, endDate;

            // 1. Procesamiento de Fecha (Soporte HTML5 Week/Month/Year)
            if (!string.IsNullOrEmpty(date))
            {
                try
                {
                    if (period == "weekly" && date.Contains("-W"))
                    {
                        var parts = date.Split("-W");
                        refDate = System.Globalization.ISOWeek.ToDateTime(int.Parse(parts[0]), int.Parse(parts[1]), DayOfWeek.Monday);
                    }
                    else if (period == "monthly" && date.Length == 7)
                    {
                        refDate = DateTime.ParseExact(date, "yyyy-MM", null);
                    }
                    else if (period == "yearly")
                    {
                        refDate = new DateTime(int.Parse(date), 1, 1);
                    }
                    else { DateTime.TryParse(date, out refDate); }
                }
                catch { refDate = DateTime.Now; }
            }
            model.DateValue = date ?? (period == "monthly" ? refDate.ToString("yyyy-MM") : refDate.ToString("yyyy-MM-dd"));

            // 2. Configuración de Rangos y Etiquetas
            List<string> labels = new List<string>();
            switch (model.SelectedPeriod.ToLower())
            {
                case "daily":
                    startDate = refDate.Date; endDate = startDate.AddDays(1).AddTicks(-1);
                    labels = Enumerable.Range(0, 24).Select(h => $"{h:00}:00").ToList();
                    break;
                case "weekly":
                    int diff = (7 + (refDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = refDate.AddDays(-1 * diff).Date; endDate = startDate.AddDays(7).AddTicks(-1);
                    labels = new List<string> { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
                    break;
                case "yearly":
                    startDate = new DateTime(refDate.Year, 1, 1); endDate = startDate.AddYears(1).AddDays(-1);
                    labels = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).Select(m => m.ToUpper()).ToList();
                    break;
                default:
                    startDate = new DateTime(refDate.Year, refDate.Month, 1); endDate = startDate.AddMonths(1).AddTicks(-1);
                    labels = Enumerable.Range(1, DateTime.DaysInMonth(refDate.Year, refDate.Month)).Select(d => $"Día {d}").ToList();
                    break;
            }

            // 3. Catálogos Dinámicos (Filtro por Sede)
            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            var productosQuery = _context.Productos.Where(p => p.ControlType == ControlType.Stock);
            if (branchId.HasValue)
            {
                var idsEnSede = await _context.ProductosStock.Where(s => s.BranchId == branchId).Select(s => s.ProductId).Distinct().ToListAsync();
                productosQuery = productosQuery.Where(p => idsEnSede.Contains(p.Id));
            }
            model.Productos = await productosQuery.OrderBy(p => p.Name).ToListAsync();

            // 4. Consulta de Movimientos
            var query = _context.MovimientosInventario.Include(m => m.Product)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate && m.Product.ControlType == ControlType.Stock);

            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);
            if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);

            var movimientos = await query.ToListAsync();

            // 5. Gráfico: Sumarizamos por Tipo de Movimiento
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

            model.GraficoLabelsJson = Newtonsoft.Json.JsonConvert.SerializeObject(labels);
            model.ValoresConsumoJson = Newtonsoft.Json.JsonConvert.SerializeObject(valoresConsumo);
            model.ValoresIngresoJson = Newtonsoft.Json.JsonConvert.SerializeObject(valoresIngreso);
            model.TotalConsumo = valoresConsumo.Sum();
            model.TotalIngreso = valoresIngreso.Sum();

            // 6. Tabla Simplificada
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

        [HttpGet]
        public async Task<IActionResult> AssetDashboard(int? branchId, string period = "monthly", string date = null)
        {
            var model = new AssetDashboardViewModel
            {
                SelectedBranchId = branchId,
                SelectedPeriod = period ?? "monthly"
            };

            // 1. Lógica de Fechas (Reutilizamos la misma del InventoryDashboard)
            DateTime refDate = DateTime.Now;
            if (!string.IsNullOrEmpty(date)) { /* ... lógica de parseo idéntica al dashboard anterior ... */ }
            model.DateValue = date ?? (period == "monthly" ? refDate.ToString("yyyy-MM") : refDate.ToString("yyyy-MM-dd"));

            // 2. Gráfico Circular: Estados de los Activos Fijos
            var assetsQuery = _context.ActivosFijos.AsQueryable();
            if (branchId.HasValue) assetsQuery = assetsQuery.Where(a => a.BranchId == branchId);

            var estados = await assetsQuery.GroupBy(a => a.Status)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() }).ToListAsync();

            model.EstadosLabelsJson = JsonConvert.SerializeObject(estados.Select(e => e.Estado));
            model.EstadosValoresJson = JsonConvert.SerializeObject(estados.Select(e => e.Cantidad));

            // 3. Tabla de Resumen por Tipo de Producto
            model.ResumenActivos = await assetsQuery.Include(a => a.Product)
                .GroupBy(a => a.Product.Name)
                .Select(g => new ResumenActivoViewModel
                {
                    ProductName = g.Key,
                    Total = g.Count(),
                    Disponibles = g.Count(x => x.Status == "Available"),
                    Asignados = g.Count(x => x.Status == "Assigned"),
                    EnMantenimiento = g.Count(x => x.Status == "Maintenance")
                }).ToListAsync();

            // 4. Catálogos y Datos Generales
            model.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();

            // Aquí podrías replicar la lógica de movimientos para ver cuántos activos 
            // entraron (altas) vs salieron (bajas/asignaciones) en el gráfico de líneas.

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetMinStockSettings(int branchId)
        {
            var stocks = await _context.ProductosStock
                .Include(ps => ps.Product)
                .Where(ps => ps.BranchId == branchId)
                .ToListAsync();

            ViewBag.BranchName = (await _context.Filiales.FindAsync(branchId))?.Name;
            return PartialView("Partials/_SetMinimumStock", stocks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMinimumStocks(List<ProductStockUpdateViewModel> model)
        {
            if (model == null || !model.Any())
                return Json(new { success = false, message = "No hay datos para actualizar." });

            foreach (var item in model)
            {
                // Buscamos el registro real en la base de datos
                var stock = await _context.ProductosStock.FindAsync(item.Id);
                if (stock != null)
                {
                    stock.MinimumStock = item.MinimumStock; // Actualizamos solo el mínimo
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Niveles de stock mínimo actualizados correctamente." });
        }

        public async Task<IActionResult> Movements()
        {
            // Cargamos los movimientos incluyendo Producto y Sede para mostrar nombres
            var movements = await _context.MovimientosInventario
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(movements);
        }

        public async Task<IActionResult> ImprimirGuia(string controlNumber)
        {
            // 1. Cargamos los movimientos incluyendo Activo Fijo para el Código Patrimonial
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.FixedAsset) // Importante para evitar el error de propiedad inexistente
                .Where(m => m.InternalControlNumber == controlNumber)
                .ToListAsync();

            if (!movimientos.Any()) return NotFound();

            var primerMov = movimientos.First();

            // 2. Construcción del Modelo Base
            var model = new ReporteGuiaViewModel
            {
                NumeroGuia = controlNumber,
                Fecha = primerMov.CreatedAt,
                UsuarioResponsable = primerMov.UserId,
                DocumentoExterno = primerMov.ExternalDocumentNumber ?? "S/N",
                // Mapeo detallado de ítems
                Items = movimientos.Select(m => new DetalleGuiaItem
                {
                    Producto = m.Product?.Name,
                    Cantidad = m.Quantity,
                    Tipo = m.MovementType == MovementType.Entry ? "ENTRADA" : "SALIDA",
                    Sede = m.Branch?.Name,
                    // Asignación del código patrimonial (si no existe, ponemos un guion)
                    CodigoPatrimonial = m.FixedAsset?.PatrimonialCode ?? "---"
                }).ToList()
            };

            // 3. Lógica específica por Concepto
            if (primerMov.Concept == Concept.Buy)
            {
                model.TipoOperacion = "ACTA DE RECEPCIÓN E INTERNAMIENTO";
                model.SedeOrigen = "PROVEEDOR / COMPRA";
                model.SedeDestino = primerMov.Branch?.Name ?? "SEDE NO DEFINIDA";
                model.EstadoMensaje = null; // En compras no suele haber estado "En tránsito"
            }
            else if (primerMov.Concept == Concept.Transfer)
            {
                model.TipoOperacion = "GUÍA DE REMISIÓN INTERNA (TRANSFERENCIA)";

                var salida = movimientos.FirstOrDefault(m => m.MovementType == MovementType.Exit);
                var entrada = movimientos.FirstOrDefault(m => m.MovementType == MovementType.Entry);

                model.SedeOrigen = salida?.Branch?.Name ?? "ORIGEN NO IDENTIFICADO";

                // Lógica de Estado "POR RECIBIR"
                if (entrada == null)
                {
                    // Buscamos la transferencia pendiente para saber a dónde se supone que debe llegar
                    var transferencia = await _context.ProductosTransferencias
                        .Include(t => t.TargetBranch)
                        .FirstOrDefaultAsync(t => t.InternalControlNumber == controlNumber);

                    model.SedeDestino = transferencia?.TargetBranch?.Name ?? "PENDIENTE DE RECEPCIÓN";
                    model.EstadoMensaje = "MERCADERÍA EN TRÁNSITO - PENDIENTE DE RECEPCIÓN";
                }
                else
                {
                    model.SedeDestino = entrada.Branch?.Name;
                    model.EstadoMensaje = "TRANSFERENCIA COMPLETADA - CARGO DE RECEPCIÓN";
                }
            }
            else
            {
                // Fallback para otros movimientos (Ajustes, Bajas, etc.)
                model.TipoOperacion = $"NOTA DE MOVIMIENTO: {primerMov.Concept}";
                model.SedeOrigen = primerMov.Branch?.Name;
                model.SedeDestino = "---";
            }

            // 4. Configuración del PDF (Vertical A4)
            return new ViewAsPdf("GuideDocumentPDF", model)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(15, 15, 15, 15),
                CustomSwitches = "--page-offset 0 --footer-center [page]/[toPage] --footer-font-size 8"
            };
        }


    }
}
