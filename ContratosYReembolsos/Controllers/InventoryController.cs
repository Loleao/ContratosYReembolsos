using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.ValueObjects;
using ContratosYReembolsos.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ContratosYReembolsos.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;

        public InventoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            // 1. Limpiamos validaciones de objetos de navegación que no vienen en el form
            ModelState.Remove("Category");
            ModelState.Remove("SubCategory");

            if (!ModelState.IsValid)
            {
                // 2. Extraemos el error exacto para mostrarlo en el SweetAlert
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return Json(new { success = false, message = "Validación fallida: " + errors });
            }

            try
            {
                if (model.Id == 0)
                {
                    _context.Productos.Add(model);
                }
                else
                {
                    // Usamos Update para que EF maneje el tracking correctamente
                    _context.Update(model);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Producto guardado correctamente en el catálogo maestro." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error de base de datos: " + ex.Message });
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
            var stockProds = await _context.Productos.Where(p => p.ControlType == ControlType.Stock).Select(p => new { id = p.Id, name = p.Name }).ToListAsync();
            var assetProds = await _context.Productos.Where(p => p.ControlType == ControlType.Asset).Select(p => new { id = p.Id, name = p.Name }).ToListAsync();

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

                foreach (var item in model.Items)
                {
                    int finalProductId = item.IsAsset ? item.ProductIdAsset.Value : item.ProductId;

                    if (item.IsAsset)
                    {
                        // --- CASO ACTIVO FIJO ---
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            var newAsset = new FixedAsset
                            {
                                ProductId = finalProductId,
                                BranchId = model.BranchId,
                                SerialNumber = item.SerialNumber ?? "S/N",
                                Status = "Available",
                                CreatedAt = DateTime.Now
                            };
                            _context.ActivosFijos.Add(newAsset);
                            await _context.SaveChangesAsync();

                            // En activos nuevos, el balance siempre es de 0 a 1 unidad
                            _context.MovimientosInventario.Add(new InventoryMovement
                            {
                                ProductId = finalProductId,
                                BranchId = model.BranchId,
                                FixedAssetId = newAsset.Id,
                                Quantity = 1,
                                PreviousQuantity = 0, // No existía antes
                                NewQuantity = 1,      // Ahora existe 1
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
                    else
                    {
                        // --- CASO STOCK CONSUMIBLE ---
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == finalProductId);

                        // Capturamos el estado inicial
                        int cantAnterior = stock?.Quantity ?? 0;
                        int cantNueva = cantAnterior + item.Quantity;

                        if (stock == null)
                        {
                            stock = new ProductStock
                            {
                                BranchId = model.BranchId,
                                ProductId = finalProductId,
                                Quantity = item.Quantity
                            };
                            _context.ProductosStock.Add(stock);
                        }
                        else
                        {
                            stock.Quantity = cantNueva;
                        }

                        // Guardamos para asegurar el ProductStockId
                        await _context.SaveChangesAsync();

                        _context.MovimientosInventario.Add(new InventoryMovement
                        {
                            ProductId = finalProductId,
                            BranchId = model.BranchId,
                            ProductStockId = stock.Id,
                            Quantity = item.Quantity,
                            PreviousQuantity = cantAnterior, // <--- Aplicado
                            NewQuantity = cantNueva,         // <--- Aplicado
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

                return Json(new { success = true, message = $"Ingreso {model.InternalControlNumber} procesado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterBulkEntry(BulkEntryViewModel model)
        //{
        //    if (model.Items == null || !model.Items.Any())
        //        return Json(new { success = false, message = "No hay ítems para procesar." });

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        string currentUserId = User.Identity?.Name ?? "Sistema";

        //        foreach (var item in model.Items)
        //        {
        //            var product = await _context.Productos.FindAsync(item.ProductId);
        //            if (product == null) continue;

        //            if (product.ControlType == ControlType.Stock)
        //            {
        //                var stock = await _context.ProductosStock
        //                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.BranchId == model.BranchId);

        //                // --- LÓGICA DE SALDOS ---
        //                int cantidadAnterior = stock?.Quantity ?? 0;
        //                int cantidadNueva = cantidadAnterior + item.Quantity;

        //                if (stock == null)
        //                {
        //                    stock = new ProductStock
        //                    {
        //                        ProductId = item.ProductId,
        //                        BranchId = model.BranchId,
        //                        Quantity = item.Quantity
        //                    };
        //                    _context.ProductosStock.Add(stock);
        //                }
        //                else
        //                {
        //                    stock.Quantity = cantidadNueva;
        //                }

        //                // Guardamos para asegurar que el ID del stock exista
        //                await _context.SaveChangesAsync();

        //                _context.MovimientosInventario.Add(new InventoryMovement
        //                {
        //                    ProductId = item.ProductId,
        //                    BranchId = model.BranchId,
        //                    ProductStockId = stock.Id,
        //                    Quantity = item.Quantity,
        //                    // Registro de Balance
        //                    PreviousQuantity = cantidadAnterior,
        //                    NewQuantity = cantidadNueva,
        //                    MovementType = MovementType.Entry,
        //                    Concept = Concept.Buy, // O el concepto que aplique
        //                    InternalControlNumber = model.InternalControlNumber, // NI-XXXX
        //                    Description = "Ingreso masivo de stock",
        //                    UserId = currentUserId
        //                });
        //            }
        //            else // Caso Asset (Activos Fijos)
        //            {
        //                // En activos, el saldo anterior siempre es 0 para ese activo específico
        //                // ya que cada registro de FixedAsset es único.
        //                for (int i = 0; i < item.Quantity; i++)
        //                {
        //                    var newAsset = new FixedAsset
        //                    {
        //                        ProductId = item.ProductId,
        //                        BranchId = model.BranchId,
        //                        SerialNumber = item.SerialNumber ?? "S/N",
        //                        PatrimonialCode = await GeneratePatrimonialCode(item.ProductId),
        //                        Status = AssetStatus.Available.ToString()
        //                    };
        //                    _context.ActivosFijos.Add(newAsset);
        //                    await _context.SaveChangesAsync();

        //                    _context.MovimientosInventario.Add(new InventoryMovement
        //                    {
        //                        ProductId = item.ProductId,
        //                        BranchId = model.BranchId,
        //                        FixedAssetId = newAsset.Id,
        //                        Quantity = 1,
        //                        PreviousQuantity = 0,
        //                        NewQuantity = 1,
        //                        MovementType = MovementType.Entry,
        //                        Concept = Concept.Buy,
        //                        InternalControlNumber = model.InternalControlNumber,
        //                        Description = "Alta masiva de activo",
        //                        UserId = currentUserId
        //                    });
        //                }
        //            }
        //        }
        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return Json(new { success = true, message = "Se procesaron todos los ingresos correctamente." });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

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
                    quantity = s.Quantity
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
                        int cantAnterior = stock.Quantity;
                        stock.Quantity -= item.Quantity;
                        int cantNueva = stock.Quantity;

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
                        int cantAnterior = stock?.Quantity ?? 0;
                        int cantNueva = cantAnterior + det.Quantity;

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

                return Json(new { success = true, message = "Mercadería recibida correctamente." });
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


        [HttpGet]
        public async Task<IActionResult> ConsumoReport(int? branchId, string period = "monthly", DateTime? date = null)
        {
            DateTime refDate = date ?? DateTime.Now;
            DateTime startDate, endDate;
            List<string> labels = new List<string>();
            List<int> valores = new List<int>();

            // 1. Configuración de Tiempos y Etiquetas del Eje X
            switch (period.ToLower())
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
                    labels = System.Globalization.DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames.Take(12).Select(m => m.ToUpper()).ToList();
                    break;
                default: // monthly
                    startDate = new DateTime(refDate.Year, refDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    labels = Enumerable.Range(1, DateTime.DaysInMonth(refDate.Year, refDate.Month)).Select(d => $"Día {d}").ToList();
                    break;
            }

            // 2. Carga de datos base
            ViewBag.Sedes = await _context.Filiales.OrderBy(b => b.Name).ToListAsync();
            ViewBag.SelectedBranch = branchId;
            ViewBag.Period = period;
            ViewBag.RefDate = refDate.ToString("yyyy-MM-dd");

            var query = _context.MovimientosInventario
                .Include(m => m.Product)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate);

            if (branchId.HasValue) query = query.Where(m => m.BranchId == branchId.Value);

            var movimientos = await query.ToListAsync();

            // 3. Lógica del Gráfico (Rellenar valores para que no haya huecos)
            var consumosRealizados = movimientos.Where(m => m.MovementType == MovementType.Exit && m.Concept != Concept.Transfer);

            for (int i = 0; i < labels.Count; i++)
            {
                int total = 0;
                if (period == "daily") total = consumosRealizados.Where(m => m.CreatedAt.Hour == i).Sum(x => x.Quantity);
                else if (period == "weekly") total = consumosRealizados.Where(m => ((int)m.CreatedAt.DayOfWeek + 6) % 7 == i).Sum(x => x.Quantity);
                else if (period == "yearly") total = consumosRealizados.Where(m => m.CreatedAt.Month == (i + 1)).Sum(x => x.Quantity);
                else total = consumosRealizados.Where(m => m.CreatedAt.Day == (i + 1)).Sum(x => x.Quantity);
                valores.Add(total);
            }

            ViewBag.GraficoLabels = Newtonsoft.Json.JsonConvert.SerializeObject(labels);
            ViewBag.GraficoValoresArray = valores; // Enviamos la lista directamente
            ViewBag.SumaTotalConsumo = valores.Sum(); // Calculamos aquí para evitar el error en la vista

            // 4. Reporte de Tabla
            var reporte = movimientos
                .GroupBy(m => new { m.ProductId, m.Product.Name, m.Product.Sku })
                .Select(g => new LineaConsumoViewModel
                {
                    ProductName = g.Key.Name,
                    Sku = g.Key.Sku,
                    StockInicial = g.OrderBy(x => x.CreatedAt).First().PreviousQuantity,
                    StockFinal = g.OrderByDescending(x => x.CreatedAt).First().NewQuantity,
                    TotalCompras = g.Where(x => x.Concept == Concept.Buy).Sum(x => x.Quantity),
                    TotalTraslados = g.Where(x => x.Concept == Concept.Transfer).Sum(x => x.MovementType == MovementType.Entry ? x.Quantity : -x.Quantity),
                    TotalConsumo = g.Where(x => x.MovementType == MovementType.Exit && x.Concept != Concept.Transfer).Sum(x => x.Quantity)
                }).ToList();

            return View(reporte);
        }
    }
}
