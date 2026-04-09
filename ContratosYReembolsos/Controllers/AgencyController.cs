using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    [Authorize]
    public class AgencyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AgencyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? selectedBranchId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            int branchId;

            if (isAdmin)
            {
                if (selectedBranchId == null)
                {
                    // Vista de selección de filiales para Admin
                    var branches = await _context.Filiales.Include(b => b.Agencies).ToListAsync();
                    return View("BranchSelection", branches);
                }
                branchId = selectedBranchId.Value;
            }
            else
            {
                branchId = user.BranchId ?? 0;
                if (branchId == 0) return Forbid();
            }

            var agencies = await _context.Agencias
                .Where(a => a.BranchId == branchId)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedBranchId = branchId;
            var branch = await _context.Filiales.FindAsync(branchId);
            ViewBag.BranchName = branch?.Name;

            return View(agencies);
        }

        [HttpGet]
        public IActionResult GetCreateAgency(int branchId)
        {
            var model = new Agency { BranchId = branchId };
            var branch = _context.Filiales.Find(branchId);
            ViewBag.BranchName = branch?.Name;

            return PartialView("Partials/_CreateAgency", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Agency model)
        {
            ModelState.Remove("Branch");

            if (ModelState.IsValid)
            {
                _context.Agencias.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "La agencia ha sido registrada correctamente." });
            }

            return Json(new { success = false, message = "Por favor, verifique los datos del formulario." });
        }

        [HttpGet]
        public async Task<IActionResult> GetEditAgency(int id)
        {
            var agency = await _context.Agencias.FindAsync(id);
            if (agency == null) return NotFound();

            var branch = await _context.Filiales.FindAsync(agency.BranchId);
            ViewBag.BranchName = branch?.Name;

            return PartialView("Partials/_EditAgency", agency);
        }

        // POST: Procesar la edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Agency model)
        {
            // 1. Limpiamos las propiedades de navegación para que no bloqueen la validación
            ModelState.Remove("Branch");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Datos de la agencia actualizados correctamente." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error en base de datos: " + ex.Message });
                }
            }

            // 2. DEBUG: Si llegamos aquí, vamos a ver EXACTAMENTE qué campo está fallando
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return Json(new { success = false, message = "Error de validación: " + errors });
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var agency = await _context.Agencias.FindAsync(id);
            if (agency == null) return Json(new { success = false, message = "Agencia no encontrada." });

            agency.IsActive = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "La agencia ha sido activada." });
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var agency = await _context.Agencias.FindAsync(id);
            if (agency == null) return Json(new { success = false, message = "Agencia no encontrada." });

            agency.IsActive = false;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "La agencia ha sido dada de baja (inactiva)." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var agency = await _context.Agencias.FindAsync(id);
                if (agency == null) return Json(new { success = false, message = "Agencia no encontrada." });

                _context.Agencias.Remove(agency);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Convenio eliminado permanentemente." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "No se puede eliminar: existen contratos vinculados a esta agencia. Considere darla de baja." });
            }
        }

    }
}