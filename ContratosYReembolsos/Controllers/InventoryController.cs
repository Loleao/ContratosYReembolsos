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
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthorizationService _authorizationService;

        public InventoryController(IInventoryService inventoryService, INotificationService notificationService, ICurrentUserService currentUser, IAuthorizationService authotizationService)
        {
            _inventoryService = inventoryService;
            _notificationService = notificationService;
            _currentUser = currentUser;
            _authorizationService = authotizationService;
        }

        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> Index()
        {
            var model = await _inventoryService.GetAdminDashboardSummary();

            // Si el usuario no es Admin, buscamos los datos de su sede asignada
            if (!_currentUser.IsAdmin && _currentUser.BranchId.HasValue)
            {
                ViewBag.UserBranch = await _inventoryService.GetBranchByIdAsync(_currentUser.BranchId.Value);
                ViewBag.UserStock = await _inventoryService.GetStockByBranchAsync(_currentUser.BranchId.Value);
            }

            return View(model);
        }

        [Authorize(Policy = "Permissions.CatalogoInventario.Ver")]
        public async Task<IActionResult> Catalog()
        {
            ViewBag.Categories = await _inventoryService.GetCategories();
            ViewBag.SubCategories = await _inventoryService.GetSubcategoriesWithCategoryAsync();
            return View(await _inventoryService.GetProductCatalog());
        }

        [Authorize(Policy = "Permissions.CatalogoInventario.Ver")]
        public async Task<IActionResult> Categories() => View(await _inventoryService.GetCategories());

        [Authorize(Policy = "Permissions.CatalogoInventario.Ver")]
        public async Task<IActionResult> Subcategories(int id)
        {
            // Usamos el ID de la categoría para cargarla junto a sus subcategorías
            var category = await _inventoryService.GetCategoryById(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryForm(int id = 0)
        {
            var model = id == 0 ? new ProductCategory() : await _inventoryService.GetCategoryById(id);
            return PartialView("Partials/_CategoryForm", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubcategoryForm(int categoryId, int id = 0)
        {
            // Pasamos las categorías al ViewBag para que el SelectList del modal se llene
            ViewBag.Categories = await _inventoryService.GetCategories();

            // Si id es 0, creamos uno nuevo vinculado a la categoría actual
            // Si no, lo buscamos en el service
            var model = id == 0
                ? new ProductSubcategory { CategoryId = categoryId }
                : await _inventoryService.GetSubCategoryById(id);

            if (model == null && id != 0) return NotFound();

            return PartialView("Partials/_SubcategoryForm", model);
        }

        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> BranchSelection()
        {
            // Obtenemos todas las sedes agrupadas (Lógica original)
            var groupedModel = await _inventoryService.GetBranchesGroupedByRegionAsync();

            // FILTRO DE SEGURIDAD: 
            // Si no es Admin, filtramos el modelo para que solo vea su propia sede
            if (!User.IsInRole("Admin"))
            {
                groupedModel = groupedModel
                    .Select(g => g.Where(b => b.Id == _currentUser.BranchId)) // Filtramos dentro del grupo
                    .Where(g => g.Any()) // Eliminamos grupos que queden vacíos
                    .SelectMany(g => g) // Aplanamos temporalmente
                    .GroupBy(b => b.Ubigeo?.Region ?? "SIN REGIÓN"); // Volvemos a agrupar
            }

            return View(groupedModel);
        }

        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> Inventory(int branchId)
        {
            // Validación de seguridad para que usuarios de sede no vean otras sedes
            if (!_currentUser.IsAdmin && _currentUser.BranchId != branchId) return Forbid();

            var sede = await _inventoryService.GetBranchByIdAsync(branchId);
            if (sede == null) return NotFound();

            var transferencias = await _inventoryService.GetPendingTransfersByBranchAsync(branchId);
            ViewBag.SedeNombre = sede.Name;
            ViewBag.BranchId = branchId;
            ViewBag.TransferenciasPendientes = transferencias;
            ViewBag.TotalPendientes = transferencias.Count;

            return View(await _inventoryService.GetStockByBranchAsync(branchId));
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> GetStockView(int branchId)
        {
            return PartialView("Partials/_StockView", await _inventoryService.GetStockByBranchAsync(branchId));
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> GetPendingView(int branchId)
        {
            return PartialView("Partials/_PendingView", await _inventoryService.GetPendingTransfersByBranchAsync(branchId));
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> GetKardexView(int branchId)
        {
            var movements = await _inventoryService.GetKardexByBranchAsync(branchId);
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
            var stock = await _inventoryService.GetAvailableItemsJsonByBranchAsync(branchId);
            return Json(new { stock });
        }

        [HttpGet]
        public async Task<IActionResult> GetStockEntry()
        {
            ViewBag.NextInternalCode = await _inventoryService.GenerateInternalCode();
            var model = new StockEntryFormViewModel
            {
                Branches = await _inventoryService.GetAllBranchesAsync()
            };
            return PartialView("Partials/_StockEntry", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductTransfer()
        {
            ViewBag.NextTransferCode = await _inventoryService.GetNextTransferCodeAsync();

            var model = new TransferFormViewModel
            {
                AllBranches = await _inventoryService.GetAllBranchesAsync(),
                IsAdmin = _currentUser.IsAdmin,
                UserBranchId = _currentUser.BranchId ?? 0,
                UserBranchName = "Sede Actual"
            };
            return PartialView("Partials/_ProductTransfer", model);
        }

        // --- PROCESAMIENTO DE DATOS ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Inventario.Ingreso")]
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
        [Authorize(Policy = "Permissions.Inventario.Transferencia")]
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

        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> Movements()
        {
            return View(await _inventoryService.GetAllMovementsAsync());
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> Kardex(int? branchId, int? productId, DateTime? start, DateTime? end)
        {
            ViewBag.Sedes = await _inventoryService.GetAllBranchesAsync();
            ViewBag.Productos = await _inventoryService.GetAllProductsAsync();

            var list = await _inventoryService.GetFilteredKardexAsync(branchId, productId, start, end);
            return View(list);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> InventoryDashboard(int? branchId, int? productId, string period = "monthly", string date = null)
        {
            var model = await _inventoryService.GetDashboardDataAsync(branchId, productId, period, date);
            return View(model);
        }

        [Authorize(Policy = "Permissions.Inventario.Ver")]
        public async Task<IActionResult> ImprimirGuia(string controlNumber)
        {
            var model = await _inventoryService.GetReporteGuiaAsync(controlNumber);
            if (model == null) return NotFound();

            return new ViewAsPdf("GuideDocumentPDF", model) { PageSize = Rotativa.AspNetCore.Options.Size.A4 };
        }

        // --- MÉTODOS DE SERVICIO RESTANTES ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(ProductCategory model)
        {
            string policy = model.Id == 0 ? "Permissions.CatalogoInventario.Crear" : "Permissions.CatalogoInventario.Editar";
            if (!(await _authorizationService.AuthorizeAsync(User, policy)).Succeeded) return Forbid();

            var result = await _inventoryService.SaveCategory(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.CatalogoInventario.Crear")]
        public async Task<IActionResult> SaveSubcategory(ProductSubcategory model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });
            var result = await _inventoryService.SaveSubcategory(model);
            return Json(new { success = result.success, message = result.message });
        }


        [HttpGet]
        [Authorize(Policy = "Permissions.Inventario.Alertas")]
        public async Task<IActionResult> GetMinStockSettings(int branchId)
        {
            var stock = await _inventoryService.GetStockByBranchAsync(branchId);
            return PartialView("Partials/_SetMinimumStock", stock);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Inventario.Alertas")]
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