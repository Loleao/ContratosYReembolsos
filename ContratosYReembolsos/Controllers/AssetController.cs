using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Models.ViewModels.Assets;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IBranchService _branchService;

        public AssetController(IAssetService assetService, IBranchService branchService)
        {
            _assetService = assetService;
            _branchService = branchService;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> BranchSelection()
        {
            // 1. Obtenemos la lista plana de sedes
            var branches = await _branchService.GetAllBranchesAsync();

            // 2. Agrupamos por Región para cumplir con el contrato de la Vista
            // Usamos el operador null-conditional (?.) para evitar errores si Ubigeo es nulo
            var groupedBranches = branches
                .GroupBy(b => b.Ubigeo?.Region ?? "SIN REGIÓN")
                .OrderBy(g => g.Key);

            // 3. Enviamos el objeto agrupado
            return View(groupedBranches);
        }

        public async Task<IActionResult> Categories() => View(await _assetService.GetCategoriesWithSub());

        public async Task<IActionResult> Subcategories(int id)
        {
            var category = await _assetService.GetCategoryById(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public async Task<IActionResult> Catalog() => View(await _assetService.GetCatalog());

        [HttpGet]
        public async Task<IActionResult> GetSubcategoriesByCategory(int categoryId)
        {
            var categories = await _assetService.GetCategoriesWithSub();
            var category = categories.FirstOrDefault(c => c.Id == categoryId);

            if (category == null) return Json(new List<object>());

            var result = category.Subcategories.Select(s => new {
                id = s.Id,
                name = s.Name
            });

            return Json(result);
        }

        // --- MÉTODOS PARA MODALES (PARTIALS) ---

        [HttpGet]
        public async Task<IActionResult> GetCategoryForm(int id = 0) =>
            PartialView("Partials/_CategoryForm", id == 0 ? new AssetCategory() : await _assetService.GetCategoryById(id));

        [HttpPost]
        public async Task<IActionResult> SaveCategory(AssetCategory model)
        {
            var success = await _assetService.SaveCategory(model);
            return Json(new { success, message = success ? "Categoría guardada correctamente." : "Error al guardar la categoría." });
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategoryForm(int categoryId, int id = 0)
        {
            var model = id == 0 ? new AssetSubcategory { CategoryId = categoryId } : await _assetService.GetSubCategoryById(id);
            var category = await _assetService.GetCategoryById(categoryId);
            ViewBag.CategoryName = category?.Name;
            return PartialView("Partials/_SubCategoryForm", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubCategory(AssetSubcategory model)
        {
            var success = await _assetService.SaveSubCategory(model);
            return Json(new { success, message = success ? "Subcategoría guardada correctamente." : "Error al guardar la subcategoría." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogForm(int id = 0)
        {
            ViewBag.Categories = await _assetService.GetCategoriesWithSub();

            if (id == 0)
            {
                return PartialView("Partials/_CatalogForm", new AssetCatalog());
            }

            // Obtenemos el item incluyendo la subcategoría para saber cuál es su CategoryId
            var model = await _assetService.GetCatalogItemById(id);
            return PartialView("Partials/_CatalogForm", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCatalogItem(AssetCatalog asset)
        {
            // Limpieza de seguridad
            ModelState.Remove("Category");
            ModelState.Remove("Subcategory");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Hay campos obligatorios vacíos." });
            }

            var success = await _assetService.SaveCatalogItem(asset);
            return Json(new { success, message = success ? "Modelo guardado con éxito." : "Error al guardar." });
        }

        // Acción para la vista principal de la sede
        public async Task<IActionResult> BranchAssets(int branchId)
        {
            // Recuperamos los activos con su respectivo molde del catálogo
            var assets = await _assetService.GetAssetsByBranch(branchId);

            // Pasamos el branchId para los botones de acción del modal
            ViewBag.BranchId = branchId;

            return View(assets);
        }

        [HttpGet]
        public async Task<IActionResult> GetRegisterEntryForm(int branchId = 0)
        {
            ViewBag.Catalog = await _assetService.GetCatalog();
            ViewBag.Branches = await _branchService.GetAllBranchesAsync();

            var model = new FixedAsset
            {
                BranchId = branchId,
            };

            return PartialView("Partials/_RegisterAssetEntry", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogJson()
        {
            var catalog = await _assetService.GetCatalog();
            var result = catalog.Select(c => new {
                id = c.Id,
                name = c.Name,
                brand = c.Brand
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetNextCode(int catalogId)
        {
            var item = await _assetService.GetCatalogItemById(catalogId);
            if (item == null) return Json(new { code = "" });
            return Json(new { code = await _assetService.GeneratePatrimonialCode(item.SubcategoryId) });
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBulkAssets(List<FixedAsset> Assets, int BranchId, string Observation)
        {
            if (Assets == null || !Assets.Any())
                return Json(new { success = false, message = "No hay activos para registrar." });

            // Limpiamos validaciones de objetos que no vienen del form
            ModelState.Remove("Branch");
            ModelState.Remove("AssetCatalog");

            var userId = User.Identity.Name ?? "System"; // Ajusta según tu sistema de Identity

            var result = await _assetService.ProcessBulkAssetEntry(Assets, BranchId, Observation, userId);

            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetTransferForm()
        {
            ViewBag.NextTransferCode = await _assetService.GenerateTransferCode();

            var model = new AssetTransferFormViewModel
            {
                AllBranches = await _branchService.GetAllBranchesAsync(),
                IsAdmin = true, // Lógica de roles de tu sistema
            };

            return PartialView("Partials/_AssetTransfer", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableAssets(int branchId)
        {
            var assets = await _assetService.GetAvailableAssetsByBranch(branchId);
            var result = assets.Select(a => new {
                id = a.Id,
                text = $"{a.PatrimonialCode} - {a.AssetCatalog.Name} (S/N: {a.SerialNumber})",
                status = a.Condition.ToString()
            });
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(AssetTransferFormViewModel model)
        {
            var userId = User.Identity.Name;
            var result = await _assetService.ProcessAssetTransfer(model, userId);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchAssetsList(int branchId)
        {
            var assets = await _assetService.GetAssetsByBranch(branchId);
            return PartialView("Partials/_BranchAssetsList", assets); // Tu vista de tabla principal
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingTransfers(int branchId)
        {
            var pending = await _assetService.GetPendingTransfersByBranch(branchId);
            return PartialView("Partials/_PendingTransfers", pending); // Tu vista de recibir
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetHistory(int branchId)
        {
            var history = await _assetService.GetKardexByBranch(branchId);
            return PartialView("Partials/_AssetKardex", history); // Tu vista de historial
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmTransferReceipt(int transferId, string receptionObservation)
        {
            var userId = User.Identity.Name;
            var result = await _assetService.ConfirmAssetReceipt(transferId, receptionObservation, userId);
            return Json(new { success = result.success, message = result.message });
        }
    }
}