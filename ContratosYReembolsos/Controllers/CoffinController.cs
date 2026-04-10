using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.External;
using ContratosYReembolsos.Models.ValueObjects;
using ContratosYReembolsos.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class CoffinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoffinController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchCode, string searchName, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            // Si NO es Admin, lo mandamos directo a SU inventario
            if (!User.IsInRole("Admin"))
            {
                if (user.BranchId.HasValue)
                {
                    return RedirectToAction("Inventory", new { id = user.BranchId.Value });
                }
                return Forbid(); // O una página de error si no tiene filial asignada
            }

            int pageSize = 8;
            IQueryable<Branch> query = _context.Filiales.AsQueryable();

            if (!string.IsNullOrEmpty(searchCode))
                query = query.Where(f => f.Code.Contains(searchCode));

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(f => f.Name.Contains(searchName));

            var totalItems = await query.CountAsync();
            var filiales = await query
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchName = searchName;

            return View(filiales); // Esta es la vista de "Logística Nacional"
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFilialesGrid(string searchCode, string searchName, int page = 1)
        {
            try
            {
                int pageSize = 8;
                IQueryable<Branch> query = _context.Filiales.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(searchCode))
                    query = query.Where(f => f.Code.Contains(searchCode) || f.Id.ToString().Contains(searchCode));

                if (!string.IsNullOrEmpty(searchName))
                    query = query.Where(f => f.Name.Contains(searchName));

                var totalItems = await query.CountAsync();
                var filiales = await query
                    .OrderBy(f => f.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                ViewBag.SearchCode = searchCode;
                ViewBag.SearchName = searchName;

                return PartialView("Partials/_SubsidiaryStock", filiales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTransfersGrid()
        {
            try
            {
                var transfers = await _context.AtaudTransferencias
                    .Include(t => t.CoffinVariant)
                        .ThenInclude(v => v.Coffin)
                    .OrderByDescending(t => t.DateSent)
                    .AsNoTracking()
                    .ToListAsync();

                return PartialView("Partials/_Transfers", transfers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR LOGISTICA: " + ex.Message);
                return StatusCode(500, "Error interno al cargar traslados");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCreateTransferStock()
        {
            // 1. Buscamos la Filial Central por su código único
            var centralBranch = await _context.Filiales
                .FirstOrDefaultAsync(f => f.Code == "LIM1");

            if (centralBranch == null) return NotFound("No se encontró la Filial Central configurada.");

            int centralId = centralBranch.Id;

            // 2. Traemos variantes con stock en la Central
            ViewBag.Products = await _context.StockFilial
                .Include(s => s.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(s => s.BranchId == centralId && s.Quantity > 0) // Cambié Quantity por CurrentStock según tu modelo nuevo
                .Select(s => new {
                    VariantId = s.CoffinVariantId,
                    DisplayName = $"{s.CoffinVariant.Coffin.ModelName} - {s.CoffinVariant.Color} ({s.CoffinVariant.Material})",
                    StockDisponible = s.Quantity
                }).ToListAsync();

            // 3. Traemos las demás filiales (Destinos posibles)
            ViewBag.Subsidiaries = await _context.Filiales
                .Where(f => f.Id != centralId)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return PartialView("Partials/_CreateTransferStock");
        }

        public async Task<IActionResult> Inventory(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Admin") && user.BranchId != id)
            {
                return Forbid();
            }

            var branch = await _context.Filiales.FindAsync(id);
            if (branch == null) return NotFound();

            ViewBag.SubsidiaryId = id;
            ViewBag.SubsidiaryName = branch.Name;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetFilialStock(int id)
        {
            var inventory = await _context.StockFilial
                .Include(s => s.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(s => s.BranchId == id)
                .Select(s => new InventoryViewModel
                {
                    VariantId = s.CoffinVariantId,
                    ModelName = s.CoffinVariant.Coffin.ModelName,
                    Color = s.CoffinVariant.Color,
                    Material = s.CoffinVariant.Material,
                    Size = s.CoffinVariant.Size,
                    ImageUrl = s.CoffinVariant.ImageUrl,
                    CurrentStock = s.Quantity,
                    MinimumStock = s.MinimumStock
                }).AsNoTracking().ToListAsync();

            ViewBag.SubsidiaryId = id;
            return PartialView("Partials/_FilialStock", inventory);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilialPendingTransfers(int id)
        {
            var pendingTransfers = await _context.AtaudTransferencias
                .Include(t => t.CoffinVariant)
                    .ThenInclude(v => v.Coffin) 
                .Include(t => t.OriginBranch) 
                .Where(t => t.TargetBranchId == id && t.Status == "EN_CAMINO")
                .OrderByDescending(t => t.DateSent)
                .ToListAsync();

            return PartialView("Partials/_FilialPendingTransfers", pendingTransfers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TransferFromGeneralBulk([FromBody] BulkTransferRequest request)
        {
            // 1. Identificar Sede Central (Origen)
            var centralBranch = await _context.Filiales.FirstOrDefaultAsync(f => f.Code == "LIM1");
            if (centralBranch == null) return Json(new { success = false, message = "No se encontró la Sede Central." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    // A. Validar y restar stock en Central
                    var sourceStock = await _context.StockFilial
                        .FirstOrDefaultAsync(s => s.BranchId == centralBranch.Id && s.CoffinVariantId == item.VariantId);

                    if (sourceStock == null || sourceStock.Quantity < item.Quantity)
                        throw new Exception($"Stock insuficiente en Central para una de las variantes seleccionadas.");

                    sourceStock.Quantity -= item.Quantity;

                    // B. Crear Movimiento de Salida (Kardex Origen)
                    var departureMovement = new CoffinMovement
                    {
                        BranchId = centralBranch.Id,
                        CoffinVariantId = item.VariantId,
                        Quantity = -item.Quantity, // Negativo porque sale
                        Type = "SALIDA_TRANSFERENCIA",
                        Reference = $"Despacho a Sede ID: {request.TargetBranchId} - Guía: {request.Reference}",
                        Date = DateTime.Now,
                        BalanceAfter = sourceStock.Quantity,
                        RegisteredBy = User.Identity?.Name ?? "Admin_Logistica"
                    };
                    _context.MovimientosAtaudes.Add(departureMovement);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el ID del movimiento

                    var transfer = new CoffinTransfer
                    {
                        CoffinVariantId = item.VariantId,
                        Quantity = item.Quantity,
                        OriginBranchId = centralBranch.Id,
                        TargetBranchId = request.TargetBranchId,
                        DepartureMovementId = departureMovement.Id, // Vinculamos la salida
                        Status = "EN_CAMINO",
                        GuiaRemision = request.Reference,
                        DateSent = DateTime.Now,
                        SentBy = User.Identity?.Name ?? "Admin_Logistica"
                    };
                    _context.AtaudTransferencias.Add(transfer);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Transferencia múltiple registrada con éxito. Los productos están en camino." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReceipt(int transferId, string? observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscar el registro de transferencia
                var transfer = await _context.AtaudTransferencias
                    .Include(t => t.CoffinVariant)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null || transfer.Status != "EN_CAMINO")
                    return Json(new { success = false, message = "Transferencia no válida o ya recibida." });

                // 2. Buscar o crear el registro de Stock en la sede destino
                var targetStock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.BranchId == transfer.TargetBranchId && s.CoffinVariantId == transfer.CoffinVariantId);

                if (targetStock == null)
                {
                    targetStock = new BranchStock
                    {
                        BranchId = transfer.TargetBranchId,
                        CoffinVariantId = transfer.CoffinVariantId,
                        Quantity = 0,
                        MinimumStock = 2,
                        LastUpdate = DateTime.Now
                    };
                    _context.StockFilial.Add(targetStock);
                }

                // 3. Actualizar el stock
                targetStock.Quantity += transfer.Quantity;
                targetStock.LastUpdate = DateTime.Now;

                // 4. Crear Movimiento de ENTRADA (Kardex Destino)
                var arrivalMovement = new CoffinMovement
                {
                    BranchId = transfer.TargetBranchId,
                    CoffinVariantId = transfer.CoffinVariantId,
                    Quantity = transfer.Quantity,
                    Type = "TRANSFERENCIA_IN",
                    Reference = $"Recepción de Sede Central - Guía: {transfer.GuiaRemision}",
                    Date = DateTime.Now,
                    BalanceAfter = targetStock.Quantity,
                    RegisteredBy = User.Identity?.Name ?? "Operario_Sede"
                };
                _context.MovimientosAtaudes.Add(arrivalMovement);
                await _context.SaveChangesAsync(); // Para obtener el ID del movimiento

                // 5. Actualizar la transferencia (Cerrar el ciclo)
                transfer.Status = "RECIBIDO";
                transfer.ArrivalMovementId = arrivalMovement.Id;
                transfer.DateReceived = DateTime.Now;
                transfer.ReceivedBy = User.Identity?.Name ?? "Operario_Sede";
                transfer.ReceptionObservations = observations;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Cargamento recibido y stock actualizado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAddStock(int branchId) 
        {
            try
            {
                var catalogData = await _context.AtaudVariantes
                    .Include(v => v.Coffin)
                    .Select(v => new {
                        v.Id,
                        Model = v.Coffin.ModelName,
                        v.Color,
                        v.Size
                    })
                    .ToListAsync();

                var variants = catalogData.Select(v => new {
                    Id = v.Id,
                    DisplayName = $"{v.Model} - {v.Color} ({v.Size})"
                }).OrderBy(v => v.DisplayName).ToList();

                var branch = await _context.Filiales.FindAsync(branchId);

                ViewBag.Variants = variants;
                ViewBag.BranchId = branchId;
                ViewBag.BranchName = branch?.Name ?? "Sede";

                return PartialView("Partials/_AddStock");
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Error interno: {ex.Message}</div>");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterBulkEntry([FromBody] BulkEntryRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return Json(new { success = false, message = "Datos inválidos." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    // OJO: Usamos request.BranchId y item.VariantId
                    var stock = await _context.StockFilial
                        .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && s.CoffinVariantId == item.VariantId);

                    if (stock == null)
                    {
                        stock = new BranchStock
                        {
                            BranchId = request.BranchId,
                            CoffinVariantId = item.VariantId,
                            Quantity = 0,
                            MinimumStock = 2,
                            LastUpdate = DateTime.Now
                        };
                        _context.StockFilial.Add(stock);
                        await _context.SaveChangesAsync();
                    }

                    stock.Quantity += item.Quantity;
                    stock.LastUpdate = DateTime.Now;

                    var movement = new CoffinMovement
                    {
                        CoffinVariantId = item.VariantId,
                        BranchId = request.BranchId,
                        Quantity = item.Quantity,
                        Type = "INGRESO_LOTE",
                        Reference = item.Reference ?? "Ingreso por lote",
                        Date = DateTime.Now,
                        BalanceAfter = stock.Quantity,
                        RegisteredBy = User.Identity?.Name ?? "Admin"
                    };
                    _context.MovimientosAtaudes.Add(movement);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Cargamento procesado con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCatalog()
        {
            var models = await _context.Ataudes
                .Include(c => c.Variants)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("Partials/_Catalog", models);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetCreateCoffin(int? coffinId, string? modelName)
        {
            var vm = new CoffinCreateViewModel();

            if (coffinId.HasValue && !string.IsNullOrEmpty(modelName))
            {
                vm.ModelName = modelName;
            }

            return PartialView("Partials/_CreateCoffin", new CoffinCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CoffinCreateViewModel vm, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return BadRequest("Información del modelo incompleta.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingCoffin = await _context.Ataudes
                    .FirstOrDefaultAsync(c => c.ModelName.ToLower() == vm.ModelName.ToLower());

                int coffinId;

                if (existingCoffin == null)
                {
                    var newCoffin = new Coffin
                    {
                        ModelName = vm.ModelName,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.Ataudes.Add(newCoffin);
                    await _context.SaveChangesAsync();
                    coffinId = newCoffin.Id;
                }
                else
                {
                    coffinId = existingCoffin.Id;
                }
                string imageUrl = "/images/coffins/default.jpg";

                var variant = new CoffinVariant
                {
                    CoffinModelId = coffinId,
                    Color = vm.Color,
                    Material = vm.Material,
                    Size = vm.Size,
                    ImageUrl = imageUrl
                };

                _context.AtaudVariantes.Add(variant);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Devolvemos éxito para que el modal se cierre y refresque el catálogo
                return Json(new { success = true, message = "Variante agregada al catálogo correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error al registrar en catálogo: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var variant = await _context.AtaudVariantes
                .Include(v => v.Stocks)
                .Include(v => v.Coffin)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variant == null) return Json(new { success = false, message = "Variante no encontrada." });

            // Regla de negocio: No borrar si hay stock registrado
            if (variant.Stocks.Any(s => s.Quantity > 0))
            {
                return Json(new { success = false, message = "No se puede borrar: Existen unidades en stock para esta variante." });
            }

            try
            {
                _context.AtaudVariantes.Remove(variant);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Variante eliminada." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "No se puede borrar: Esta variante ya tiene historial de movimientos (Kardex)." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCoffin(int id)
        {
            var coffin = await _context.Ataudes
                .Include(c => c.Variants)
                .ThenInclude(v => v.Stocks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coffin == null) return Json(new { success = false, message = "Modelo no encontrado." });

            // Verificar si alguna de sus variantes tiene stock
            if (coffin.Variants.Any(v => v.Stocks.Any(s => s.Quantity > 0)))
            {
                return Json(new { success = false, message = "No se puede borrar el modelo: Una o más variantes tienen stock físico." });
            }

            try
            {
                _context.Ataudes.Remove(coffin);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Modelo y todas sus variantes eliminados." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "No se puede borrar: El modelo ya está vinculado a operaciones logísticas." });
            }
        }
    }
}
