using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.Entities.Agencies;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContratosYReembolsos.Controllers
{
    [Authorize]
    public class AgencyController : Controller
    {
        private readonly IAgencyService _agencyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyController(IAgencyService agencyService, UserManager<ApplicationUser> userManager)
        {
            _agencyService = agencyService;
            _userManager = userManager;
        }

        [Authorize(Policy = "Permissions.Convenios.Ver")]
        public async Task<IActionResult> Index(int? selectedBranchId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            int branchId;

            if (isAdmin)
            {
                // Si es Admin y no ha seleccionado una filial, lo enviamos a la selección
                if (selectedBranchId == null)
                {
                    var branches = await _agencyService.GetBranchesWithAgencies();
                    var groupedBranches = branches
                        .GroupBy(b => b.Ubigeo?.Region ?? "SIN REGIÓN")
                        .OrderBy(g => g.Key);

                    return View("BranchSelection", groupedBranches);
                }
                branchId = selectedBranchId.Value;
            }
            else
            {
                // Si NO es Admin, ignoramos cualquier ID externo y usamos el de su perfil
                branchId = user.BranchId ?? 0;

                if (branchId == 0)
                {
                    return Forbid(); // Usuario sin sede asignada no puede ver convenios
                }
            }

            // Datos para la vista de listado de convenios
            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedBranchId = branchId;
            ViewBag.BranchName = await _agencyService.GetBranchName(branchId);

            var agencies = await _agencyService.GetAgenciesByBranch(branchId);
            return View(agencies);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Convenios.Crear")]
        public async Task<IActionResult> GetCreateAgency(int branchId)
        {
            ViewBag.BranchName = await _agencyService.GetBranchName(branchId);
            return PartialView("Partials/_CreateAgency", new Agency { BranchId = branchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Convenios.Crear")]
        public async Task<IActionResult> Create(Agency model)
        {
            ModelState.Remove("Branch");
            if (!ModelState.IsValid) return Json(new { success = false, message = "Verifique los datos." });

            var result = await _agencyService.Create(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Convenios.Editar")]
        public async Task<IActionResult> GetEditAgency(int id)
        {
            var agency = await _agencyService.GetById(id);
            if (agency == null) return NotFound();

            ViewBag.BranchName = await _agencyService.GetBranchName(agency.BranchId);
            return PartialView("Partials/_EditAgency", agency);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Convenios.Editar")]
        public async Task<IActionResult> Edit(Agency model)
        {
            ModelState.Remove("Branch");
            if (!ModelState.IsValid) return Json(new { success = false, message = "Error de validación." });

            var result = await _agencyService.Edit(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Agregado para consistencia y seguridad
        [Authorize(Policy = "Permissions.Convenios.Editar")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _agencyService.ToggleStatus(id, true);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Agregado para consistencia y seguridad
        [Authorize(Policy = "Permissions.Convenios.Editar")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _agencyService.ToggleStatus(id, false);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Para que sea consistente con el envío del token en JS
        [Authorize(Policy = "Permissions.Convenios.Eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _agencyService.Delete(id);
            return Json(new { success = result.success, message = result.message });
        }
    }
}