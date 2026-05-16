using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class CemeteryController : Controller
    {
        private readonly ICemeteryService _cemeteryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Solo para ViewBags rápidos de filiales

        public CemeteryController(ICemeteryService cemeteryService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _cemeteryService = cemeteryService;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Policy = "Permissions.Cementerios.Ver")]
        public async Task<IActionResult> Index(int? selectedBranchId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            int branchId;

            if (isAdmin)
            {
                // Si es Admin y no ha seleccionado una sede, mostramos el mapa de selección
                if (selectedBranchId == null)
                {
                    var groupedBranches = await _cemeteryService.GetBranchesGroupedByRegionAsync();
                    return View("BranchSelection", groupedBranches);
                }
                branchId = selectedBranchId.Value;
            }
            else
            {
                // Si NO es Admin, ignoramos cualquier intento de inyección de ID y usamos su sede asignada
                branchId = user.BranchId ?? 0;
                if (branchId == 0) return Forbid();
            }

            // Obtenemos los datos necesarios para el Header de la vista
            // Es mejor pedir el objeto Branch al servicio para no usar el Context aquí
            var branch = await _cemeteryService.GetBranchByIdAsync(branchId);

            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedBranchId = branchId;
            ViewBag.BranchName = branch?.Name;

            var cemeteries = await _cemeteryService.GetCemeteriesByBranch(branchId);
            return View(cemeteries);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Cementerios.Crear")]
        public async Task<IActionResult> GetCreateCemetery(int branchId)
        {
            var branch = await _context.Filiales.FindAsync(branchId);
            ViewBag.BranchName = branch?.Name ?? "Filial";
            return PartialView("Partials/_CreateCemetery", new Cemetery { BranchId = branchId, IsActive = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Cementerios.Crear")]
        public async Task<IActionResult> Create(Cemetery model)
        {
            // Quitamos UbigeoId y las navegación de la validación inicial
            ModelState.Remove("UbigeoId");
            ModelState.Remove("Ubigeo");
            ModelState.Remove("Branch");
            ModelState.Remove("Structures");

            if (model.BranchId == 0)
                return Json(new { success = false, message = "Error: No se detectó filial." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Verifique: " + errors });
            }

            var result = await _cemeteryService.CreateCemetery(model);
            return Json(new { success = result.success, message = result.message });
        }

        public async Task<IActionResult> SelectionMenu(int id)
        {
            var cemetery = await _cemeteryService.GetById(id);
            if (cemetery == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsInRoleAsync(user, "Admin") && cemetery.BranchId != user.BranchId) return Forbid();
            return View(cemetery);
        }

        public async Task<IActionResult> InventoryList(int id, string type)
        {
            var cemetery = await _cemeteryService.GetById(id);
            if (cemetery == null) return NotFound();

            ViewBag.CemeteryId = id;
            ViewBag.CemeteryName = cemetery.Name;
            ViewBag.CurrentType = type.ToUpper();
            ViewBag.Templates = await _cemeteryService.GetTemplatesByType(type);

            return View(await _cemeteryService.GetStructuresByCemeteryAndType(id, type));
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Estructuras.Crear")]
        public async Task<IActionResult> GetBuildStructure(int id, string type)
        {
            var cemetery = await _cemeteryService.GetById(id);
            if (cemetery == null) return NotFound();

            ViewBag.CemeteryId = id;
            ViewBag.CemeteryName = cemetery.Name;
            ViewBag.CurrentType = type.ToUpper();
            ViewBag.Templates = await _cemeteryService.GetTemplatesByType(type);

            return PartialView("Partials/_BuildStructure", new IntermentStructure { CemeteryId = id, Type = type.ToUpper() });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Estructuras.Crear")]
        public async Task<IActionResult> BuildStructure(IntermentStructure model, int? templateId)
        {
            var result = await _cemeteryService.BuildStructure(model, templateId);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public async Task<IActionResult> GetAddMassiveGraves(int structureId)
        {
            var structure = await _context.SepulturasEstructura
                .Include(s => s.Spaces)
                .FirstOrDefaultAsync(s => s.Id == structureId);

            if (structure == null) return NotFound("La estructura no existe.");

            ViewBag.StructureId = structureId;

            // Agrupamos por letra de fila para saber qué filas ya existen
            var groups = structure.Spaces
                .GroupBy(s => s.RowLetter)
                .OrderBy(g => g.Key)
                .ToList();

            ViewBag.ExistingRows = groups;

            // Lógica para sugerir la siguiente letra (Ej: si existe A y B, sugiere C)
            string nextLetter = "A";
            if (groups.Any())
            {
                string lastLetter = groups.Last().Key;
                if (!string.IsNullOrEmpty(lastLetter))
                {
                    char lastChar = lastLetter.ToUpper()[0];
                    nextLetter = ((char)(lastChar + 1)).ToString();
                }
            }
            ViewBag.NextLetter = nextLetter;

            return PartialView("Partials/_AddMassiveGraves");
        }


        [HttpPost]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public async Task<IActionResult> AddMassiveTumbas(int structureId, string rowLetter, int quantity)
        {
            var result = await _cemeteryService.AddMassiveSpaces(structureId, rowLetter, quantity);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Espacios.Eliminar")]
        public async Task<IActionResult> DeleteSpace(int id)
        {
            var result = await _cemeteryService.DeleteSpace(id);
            return Json(new { success = result.success, message = result.message });
        }

        public async Task<IActionResult> Details(int id)
        {
            var structure = await _cemeteryService.GetStructureDetails(id);
            return structure == null ? NotFound() : View(structure);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(int originSpaceId, int destinationSpaceId, string reason)
        {
            var result = await _cemeteryService.ProcessTransfer(originSpaceId, destinationSpaceId, reason);
            return Json(new { success = result.success, message = result.message });
        }

        // Métodos de templates y manuales simplificados

        [HttpGet]
        [Authorize(Policy = "Permissions.Modelos.Ver")]
        public async Task<IActionResult> GetTemplateForm(int id = 0)
        {
            // Si el ID es 0, enviamos una entidad nueva (Crear)
            // Si es > 0, buscamos la existente en el servicio (Editar)
            var model = id == 0
                ? new IntermentStructureTemplate()
                : await _context.TemplatesSepulturas.FindAsync(id); // O usar un método del Service

            return PartialView("Partials/_TemplateForm", model);
        }

        [Authorize(Policy = "Permissions.Modelos.Ver")]
        public async Task<IActionResult> Templates()
        {
            // Pedimos los datos al servicio
            var templates = await _cemeteryService.GetTemplates();

            // Pasamos la lista a la vista
            return View(templates);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Modelos.Crear")] // El service debería manejar si es update o insert
        public async Task<IActionResult> SaveTemplate(IntermentStructureTemplate model)
        {
            // Ajustar el Service para que haga Update si el Id > 0
            var result = await _cemeteryService.SaveTemplate(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Modelos.Eliminar")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var result = await _cemeteryService.DeleteTemplate(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public async Task<IActionResult> AddManualSpace(int structureId, string row, int col) => Json(await _cemeteryService.AddManualSpace(structureId, row, col));
        
        [HttpGet]
        [Authorize(Policy = "Permissions.Modelos.Crear")]
        public IActionResult GetCreateTemplate() => PartialView("Partials/_CreateTemplate", new IntermentStructureTemplate());

        [HttpGet]
        public async Task<IActionResult> GetSpaceHistory(int spaceId)
        {
            if (spaceId <= 0) return BadRequest("ID de espacio inválido.");

            var history = await _cemeteryService.GetSpaceHistoryAsync(spaceId);
            return Json(history);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Cementerios.Ver")]
        public async Task<IActionResult> SpaceHistoryReport(int id)
        {
            if (id <= 0) return NotFound();

            var reportDto = await _cemeteryService.GetSpaceReportDetailAsync(id);
            if (reportDto == null) return NotFound("No se encontró información técnica para el espacio solicitado.");

            return View(reportDto);
        }

    }
}