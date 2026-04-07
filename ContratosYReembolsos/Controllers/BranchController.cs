using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class BranchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var branches = await _context.Filiales
                .Include(b => b.Ubigeo)
                .Include(b => b.Cemeteries)
                .ToListAsync();

            // Agrupamos por Region en lugar de Department
            var groupedBranches = branches
                .OrderBy(b => b.Ubigeo.Region)
                .GroupBy(b => b.Ubigeo.Region)
                .ToList();

            return View(groupedBranches);
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateBranch()
        {
            // Solo cargamos los departamentos únicos para el primer select
            ViewBag.Departamentos = await _context.Ubigeos
                .Select(u => u.Region)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            return PartialView("Partials/_CreateBranch", new Branch());
        }

        [HttpGet]
        public async Task<JsonResult> GetProvincias(string region)
        {
            var provincias = await _context.Ubigeos
                .Where(u => u.Region == region)
                .Select(u => u.Province)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
            return Json(provincias);
        }

        [HttpGet]
        public async Task<JsonResult> GetDistritos(string region, string provincia)
        {
            var distritos = await _context.Ubigeos
                .Where(u => u.Region == region && u.Province == provincia)
                .Select(u => new { u.Id, u.District })
                .OrderBy(d => d.District)
                .ToListAsync();
            return Json(distritos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch model)
        {
            // 1. IMPORTANTE: Ignoramos la validación de estos campos para que ModelState sea True
            ModelState.Remove("Code");
            ModelState.Remove("Ubigeo");

            if (ModelState.IsValid)
            {
                // 2. Buscamos el Ubigeo seleccionado para saber su Región
                var ubigeo = await _context.Ubigeos.FindAsync(model.UbigeoId);
                if (ubigeo == null) return Json(new { success = false, message = "Ubigeo no encontrado." });

                // 3. Obtenemos la abreviatura (Ej: LIM, APU, JUN)
                var prefix = await _context.Ubigeos
                    .Where(u => u.Region == ubigeo.Region && !string.IsNullOrEmpty(u.Abbreviation))
                    .Select(u => u.Abbreviation)
                    .FirstOrDefaultAsync() ?? "GEN";

                // 4. Calculamos el siguiente número correlativo
                // Buscamos cuántas sedes existen ya con ese prefijo
                var lastNumber = await _context.Filiales
                    .Where(b => b.Code.StartsWith(prefix))
                    .CountAsync();

                // 5. ASIGNAMOS EL CÓDIGO FINAL (Aquí es donde ocurre la magia)
                model.Code = $"{prefix}{lastNumber + 1}";

                _context.Filiales.Add(model);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Filial {model.Code} creada con éxito." });
            }

            // Si hay errores de validación, los mandamos al SweetAlert para saber qué falló
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = "Errores: " + errors });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Filiales
                .Include(b => b.Ubigeo)      // Carga la ubicación (Region, Prov, Dist)
                .Include(b => b.Cemeteries)  // Carga la lista de cementerios vinculados
                .FirstOrDefaultAsync(m => m.Id == id);

            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpGet]
        public async Task<IActionResult> GetEditBranch(int id)
        {
            var branch = await _context.Filiales
                .Include(b => b.Ubigeo)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return NotFound();

            return PartialView("Partials/_EditBranch", branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Branch model)
        {
            ModelState.Remove("Ubigeo");
            ModelState.Remove("Cemeteries");
            ModelState.Remove("Code");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Filial actualizada correctamente." });
            }

            return Json(new { success = false, message = "Error al validar los datos." });
        }
    }
}
