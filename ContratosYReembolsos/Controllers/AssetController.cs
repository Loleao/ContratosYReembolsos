using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Services.Implementations.Branches;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContratosYReembolsos.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var branches = await _branchService.GetGroupedBranchesAsync();
            return View(branches);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _assetService.GetCategories();
            return View(categories);
        }

        public async Task<IActionResult> Subcategories()
        {
            var subcategories = await _assetService.GetSubcategories();
            return View(subcategories);
        }

        public async Task<IActionResult> Catalog()
        {
            var catalog = await _assetService.GetCatalog();
            return View(catalog);
        }

        // --- CARGA DE MODALES (PARTIALS) ---
        [HttpGet]
        public async Task<IActionResult> GetCategoryForm(int id = 0)
        {
            var model = id == 0 ? new AssetCategory() : (await _assetService.GetCategories()).FirstOrDefault(c => c.Id == id);
            return PartialView("Partials/_CategoryForm", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogForm(int id = 0)
        {
            ViewBag.Categories = await _assetService.GetCategories();
            var model = id == 0 ? new AssetCatalog() : (await _assetService.GetCatalog()).FirstOrDefault(c => c.Id == id);
            return PartialView("Partials/_CatalogForm", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetRegisterAssetEntry(int branchId)
        {
            ViewBag.Catalog = await _assetService.GetCatalog();
            var model = new FixedAsset { BranchId = branchId };
            return PartialView("Partials/_RegisterAssetEntry", model);
        }

        // --- ACCIONES POST ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(AssetCategory model)
        {
            // Lógica para guardar categoría
            // Implementar en Service: Task<bool> SaveCategory(AssetCategory model);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCatalogItem(AssetCatalog model, IFormFile ImageFile)
        {
            if (ImageFile != null) { /* Lógica de guardado de imagen */ }
            var success = await _assetService.SaveCatalogItem(model);
            return Json(new { success });
        }
    }
}