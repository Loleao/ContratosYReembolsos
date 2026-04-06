using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class CemeteryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IntermentService _intermentService;

        public CemeteryController(ApplicationDbContext context, IntermentService intermentService)
        {
            _context = context;
            _intermentService = intermentService;
        }

        // Dashboard principal o listado filtrado
        public async Task<IActionResult> Index()
        {
            var cemeteries = await _context.Cementerios
                .Include(c => c.Structures)
                .ToListAsync();
            return View(cemeteries);
        }

        [HttpGet]
        public IActionResult GetCreateCemetery()
        {
            return PartialView("Partials/_CreateCemetery", new Cemetery());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cemetery model)
        {
            if (ModelState.IsValid)
            {
                // El ID al ser int e Identity no se asigna manualmente
                _context.Cementerios.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Sede {model.Name} registrada con éxito." });
            }

            // Si hay errores de validación
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = "Error de validación: " + errors });
        }

        public async Task<IActionResult> SelectionMenu(int id)
        {
            var cemetery = await _context.Cementerios.FindAsync(id);
            if (cemetery == null) return NotFound();

            return View(cemetery);
        }

        public async Task<IActionResult> InventoryList(int id, string type)
        {
            if (string.IsNullOrEmpty(type)) return RedirectToAction("SelectionMenu", new { id });

            var cemetery = await _context.Cementerios.FindAsync(id);
            if (cemetery == null) return NotFound();

            ViewBag.CemeteryId = id;
            ViewBag.CemeteryName = cemetery.Name;
            ViewBag.CurrentType = type.ToUpper();

            // Cargamos templates del tipo seleccionado
            ViewBag.Templates = await _context.TemplatesSepulturas
                .Where(t => t.Type == type.ToUpper())
                .ToListAsync();

            // Cargamos estructuras de esa sede y de ese tipo
            var structures = await _context.SepulturasEstructura
                .Include(s => s.Spaces)
                .Where(s => s.CemeteryId == id && s.Type == type.ToUpper())
                .ToListAsync();

            return View(structures);
        }

        public async Task<IActionResult> Templates()
        {
            var templates = await _context.TemplatesSepulturas.ToListAsync();
            return View(templates);
        }

        [HttpGet]
        public IActionResult GetCreateTemplate()
        {
            var model = new IntermentStructureTemplate();
            return PartialView("Partials/_CreateTemplate", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetBuildStructure(int id, string type)
        {
            var cemetery = await _context.Cementerios.FindAsync(id);

            if (cemetery == null) return NotFound("No se encontró el cementerio");

            ViewBag.CemeteryId = id;
            ViewBag.CemeteryName = cemetery.Name;
            ViewBag.CurrentType = type.ToUpper();

            ViewBag.Templates = await _context.TemplatesSepulturas
                .Where(t => t.Type == type.ToUpper())
                .ToListAsync();

            var model = new IntermentStructure
            {
                CemeteryId = id,
                Type = type.ToUpper()
            };
            return PartialView("Partials/_BuildStructure", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTemplate(IntermentStructureTemplate model)
        {
            if (ModelState.IsValid)
            {
                _context.TemplatesSepulturas.Add(model);
                await _context.SaveChangesAsync();
                // Devolvemos JSON para que AJAX sepa que terminó bien
                return Json(new { success = true, message = "Modelo guardado con éxito." });
            }
            return Json(new { success = false, message = "Datos inválidos en el formulario." });
        }

        [HttpPost]
        public async Task<IActionResult> BuildStructure(IntermentStructure model, int? templateId)
        {
            try
            {
                // El modelo ya trae CemeteryId desde el formulario (campo hidden)
                _context.SepulturasEstructura.Add(model);
                await _context.SaveChangesAsync();

                if (templateId.HasValue)
                {
                    await _intermentService.BuildFromTemplateAsync(templateId.Value, model.Id);
                }

                return Json(new { success = true, message = "Estructura creada en la sede." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddManualSpace(int structureId, string row, int col)
        {
            var structure = await _context.SepulturasEstructura.FindAsync(structureId);
            if (structure == null) return Json(new { success = false, message = "Estructura no encontrada." });

            var newSpace = new IntermentSpace
            {
                StructureId = structureId,
                RowLetter = row,
                ColumnNumber = col,
                Code = $"{structure.Name.Substring(0, 3).ToUpper()}-T-{row}{col}",
                Status = IntermentStatus.Disponible,
                Price = 2000
            };

            _context.SepulturasNichos.Add(newSpace);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAddGrave(int structureId)
        {
            var structure = await _context.SepulturasEstructura.FindAsync(structureId);
            ViewBag.StructureName = structure?.Name;

            var model = new IntermentSpace { StructureId = structureId };
            return PartialView("Partials/_AddGrave", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAddMassiveGraves(int structureId)
        {
            var structure = await _context.SepulturasEstructura
                .Include(s => s.Spaces)
                .FirstOrDefaultAsync(s => s.Id == structureId);

            ViewBag.StructureId = structureId;
            var groups = structure.Spaces.GroupBy(s => s.RowLetter).OrderBy(g => g.Key).ToList();
            ViewBag.ExistingRows = groups;

            // Lógica para la siguiente letra
            string nextLetter = "A";
            if (groups.Any())
            {
                char lastChar = groups.Last().Key[0];
                nextLetter = ((char)(lastChar + 1)).ToString();
            }
            ViewBag.NextLetter = nextLetter;

            return PartialView("Partials/_AddMassiveGraves");
        }

        [HttpPost]
        public async Task<IActionResult> AddMassiveTumbas(int structureId, string rowLetter, int quantity)
        {
            try
            {
                var structure = await _context.SepulturasEstructura
                    .Include(s => s.Spaces)
                    .FirstOrDefaultAsync(s => s.Id == structureId);

                if (structure == null) return Json(new { success = false, message = "Estructura no encontrada." });

                rowLetter = rowLetter.ToUpper().Trim();

                // Buscamos el último número ocupado en esa fila
                int lastCol = structure.Spaces
                    .Where(s => s.RowLetter == rowLetter)
                    .Select(s => s.ColumnNumber)
                    .DefaultIfEmpty(0)
                    .Max();

                for (int i = 1; i <= quantity; i++)
                {
                    int nextNumber = lastCol + i;
                    var newSpace = new IntermentSpace
                    {
                        StructureId = structureId,
                        RowLetter = rowLetter,
                        ColumnNumber = nextNumber,
                        Code = $"{structure.Name.Substring(0, 2).ToUpper()}-{rowLetter}-{nextNumber:D2}",
                        Status = IntermentStatus.Disponible,
                        FloorNumber = 1
                    };
                    _context.SepulturasNichos.Add(newSpace);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Se generaron {quantity} espacios en la Fila {rowLetter} con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpace(int id)
        {
            try
            {
                var space = await _context.SepulturasNichos.FindAsync(id);
                if (space == null) return Json(new { success = false, message = "Espacio no encontrado." });

                if (space.Status != IntermentStatus.Disponible)
                {
                    return Json(new { success = false, message = "No se puede eliminar un espacio que no está disponible (Ocupado/Reservado)." });
                }

                _context.SepulturasNichos.Remove(space);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Espacio eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var structure = await _context.SepulturasEstructura
                .Include(s => s.Template)
                .Include(s => s.Spaces)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (structure == null) return NotFound();

            // Agrupamos los espacios por Piso para el renderizado
            return View(structure);
        }

        [HttpGet]
        public async Task<IActionResult> GetExhumation(int spaceId)
        {
            var originSpace = await _context.SepulturasNichos
                .Include(s => s.Structure)
                .FirstOrDefaultAsync(s => s.Id == spaceId);

            if (originSpace == null || originSpace.Status != IntermentStatus.Ocupado)
                return Json(new { success = false, message = "El espacio no tiene un difunto para exhumar." });

            // Aquí buscarías el registro del difunto asociado al SpaceId
            // var deceased = await _context.Difuntos.FirstOrDefaultAsync(d => d.CurrentSpaceId == spaceId);

            ViewBag.OriginSpace = originSpace;
            return PartialView("Partials/_Exhumation");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(int originSpaceId, int destinationSpaceId, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var origin = await _context.SepulturasNichos.FindAsync(originSpaceId);
                var destination = await _context.SepulturasNichos.FindAsync(destinationSpaceId);

                if (destination.Status != IntermentStatus.Disponible)
                    return Json(new { success = false, message = "El destino ya no está disponible." });

                // 1. Liberar Origen
                origin.Status = IntermentStatus.Disponible;

                // 2. Ocupar Destino
                destination.Status = IntermentStatus.Ocupado;

                // 3. Registrar el Movimiento (Historial)
                // var log = new MovementLog { ... };

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Traslado completado con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}