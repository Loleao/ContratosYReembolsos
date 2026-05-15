using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContratosYReembolsos.Controllers
{
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService) => _branchService = branchService;

        [Authorize(Policy = "Permissions.Filiales.Ver")]
        public async Task<IActionResult> Index()
            => View(await _branchService.GetGroupedBranchesAsync());

        [HttpGet]
        [Authorize(Policy = "Permissions.Filiales.Crear")]
        public async Task<IActionResult> GetCreateBranch()
        {
            ViewBag.Departamentos = await _branchService.GetRegionsAsync();
            return PartialView("Partials/_CreateBranch", new Branch());
        }

        [HttpGet]
        public async Task<JsonResult> GetProvincias(string region)
            => Json(await _branchService.GetProvincesAsync(region));

        [HttpGet]
        public async Task<JsonResult> GetDistritos(string region, string provincia)
            => Json(await _branchService.GetDistrictsAsync(region, provincia));

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Filiales.Crear")]
        public async Task<IActionResult> Create(Branch model)
        {
            ModelState.Remove("Code");
            ModelState.Remove("Ubigeo");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Errores: " + errors });
            }

            var result = await _branchService.CreateBranchAsync(model);
            return Json(new { success = result.success, message = result.message });
        }

        [Authorize(Policy = "Permissions.Filiales.Ver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var branch = await _branchService.GetByIdAsync(id.Value);
            return branch == null ? NotFound() : View(branch);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Filiales.Editar")]
        public async Task<IActionResult> GetEditBranch(int id)
        {
            var branch = await _branchService.GetByIdAsync(id);
            return branch == null ? NotFound() : PartialView("Partials/_EditBranch", branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Filiales.Editar")]
        public async Task<IActionResult> Edit(Branch model)
        {
            ModelState.Remove("Ubigeo");
            ModelState.Remove("Cemeteries");
            ModelState.Remove("Code");

            if (!ModelState.IsValid) return Json(new { success = false, message = "Error al validar los datos." });

            var result = await _branchService.UpdateBranchAsync(model);
            return Json(new { success = result.success, message = result.message });
        }
    }
}