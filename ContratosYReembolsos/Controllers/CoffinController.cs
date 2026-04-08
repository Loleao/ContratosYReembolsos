using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.External;
using ContratosYReembolsos.Models.ViewModels;
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

        // PASO 1: Mostrar todas las Filiales (codfilial, descripcion...)
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

            // Seguridad: Si no es admin y quiere ver otra filial, bloqueamos
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

        // 2. Parcial: Stock Real de la Filial
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

        // 3. Parcial: Transferencias por Recibir
        [HttpGet]
        public async Task<IActionResult> GetFilialPendingTransfers(int id)
        {
            var pending = await _context.AtaudTransferencias
                .Include(t => t.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(t => t.TargetBranchId == id && t.Status == "EN_CAMINO")
                .AsNoTracking().ToListAsync();

            ViewBag.SubsidiaryId = id;
            return PartialView("Partials/_FilialPendingTransfers", pending);
        }

        [HttpPost]
        public async Task<IActionResult> TransferFromGeneral(int variantId, int targetSubsidiaryId, int quantity, string reference)
        {
            // 1. Buscamos dinámicamente el ID de la central por su código "001"
            var centralBranch = await _context.Filiales.FirstOrDefaultAsync(f => f.Code == "LIM1");

            if (centralBranch == null)
                return Json(new { success = false, message = "Error: La Sede Central no está configurada." });

            int centralId = centralBranch.Id;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Validar Stock en Origen (Usando CurrentStock)
                var centralStock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == variantId && s.BranchId == centralId);

                if (centralStock == null || centralStock.Quantity < quantity)
                    return Json(new { success = false, message = "Stock insuficiente en Central." });

                // 3. Ejecutar Salida Física
                centralStock.Quantity -= quantity;
                centralStock.LastUpdate = DateTime.Now;

                // 4. Crear el Movimiento de Salida (Kardex)
                var moveOut = new CoffinMovement
                {
                    CoffinVariantId = variantId,
                    BranchId = centralId, // Usamos el ID numérico
                    Quantity = -quantity, // Negativo porque sale del almacén
                    Type = "TRANSFERENCIA_OUT",
                    Reference = reference,
                    Date = DateTime.Now,
                    BalanceAfter = centralStock.Quantity,
                    RegisteredBy = User.Identity?.Name ?? "Admin"
                };

                _context.MovimientosAtaudes.Add(moveOut);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id de moveOut

                // 5. Crear la Transferencia Logística
                var transfer = new CoffinTransfer
                {
                    CoffinVariantId = variantId,
                    OriginBranchId = centralId,
                    TargetBranchId = targetSubsidiaryId, // Ya viene como int desde el modal
                    Quantity = quantity,
                    Status = "EN_CAMINO",
                    GuiaRemision = reference,
                    DateSent = DateTime.Now,
                    SentBy = User.Identity?.Name ?? "Admin",
                    DepartureMovementId = moveOut.Id // Puntero al Kardex de salida
                };

                _context.AtaudTransferencias.Add(transfer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Envío procesado. El stock ahora figura 'En Tránsito'." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Es mejor loguear el error interno y mandar un mensaje amigable
                return Json(new { success = false, message = "Error al procesar traslado: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReceipt(int transferId, string? observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Obtener la transferencia pendiente
                var transfer = await _context.AtaudTransferencias.FindAsync(transferId);
                if (transfer == null || transfer.Status != "EN_CAMINO")
                    return Json(new { success = false, message = "Transferencia no encontrada o ya procesada." });

                // 2. Buscar o crear el Stock en la Filial Destino
                var targetStock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == transfer.CoffinVariantId && s.BranchId == transfer.TargetBranchId);

                if (targetStock == null)
                {
                    targetStock = new BranchStock
                    {
                        CoffinVariantId = transfer.CoffinVariantId,
                        BranchId = transfer.TargetBranchId,
                        Quantity = 0,
                        MinimumStock = 0, // Se puede ajustar después
                        LastUpdate = DateTime.Now
                    };
                    _context.StockFilial.Add(targetStock);
                }

                // 3. Ejecutar Entrada Física
                targetStock.Quantity += transfer.Quantity;
                targetStock.LastUpdate = DateTime.Now;

                // 4. Crear el Movimiento de Entrada (Kardex)
                var moveIn = new CoffinMovement
                {
                    CoffinVariantId = transfer.CoffinVariantId,
                    BranchId = transfer.TargetBranchId,
                    Quantity = transfer.Quantity, // Positivo porque entra
                    Type = "TRANSFERENCIA_IN",
                    Reference = transfer.GuiaRemision,
                    Date = DateTime.Now,
                    BalanceAfter = targetStock.Quantity,
                    RegisteredBy = User.Identity?.Name ?? "UserFilial"
                };
                _context.MovimientosAtaudes.Add(moveIn);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id de moveIn

                // 5. Cerrar la Transferencia (Tu idea de los 2 campos)
                transfer.ArrivalMovementId = moveIn.Id; // <-- Puntero al movimiento que finaliza el proceso
                transfer.Status = "RECIBIDO";
                transfer.DateReceived = DateTime.Now;
                transfer.ReceivedBy = User.Identity?.Name ?? "UserFilial";
                transfer.ReceptionObservations = observations;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Stock recibido y cargado al inventario local." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error técnico: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAddStock(int variantId, int subsidiaryId)
        {
            var variant = await _context.AtaudVariantes
                .Include(v => v.Coffin)
                .FirstOrDefaultAsync(v => v.Id == variantId);

            var filial = await _context.Filiales.FirstOrDefaultAsync(f => f.Id == subsidiaryId);

            if (variant == null || filial == null) return NotFound();

            ViewBag.VariantId = variant.Id;
            ViewBag.ModelName = $"{variant.Coffin.ModelName} ({variant.Color})";
            ViewBag.SubsidiaryName = filial.Name;
            ViewBag.SubsidiaryId = subsidiaryId;

            return PartialView("Partials/_AddStock");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterEntry(int variantId, int branchId, int quantity, string reference)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == variantId && s.BranchId == branchId);

                if (stock == null)
                {
                    stock = new BranchStock
                    {
                        CoffinVariantId = variantId,
                        BranchId = branchId,
                        Quantity = 0,
                        MinimumStock = 5,
                        LastUpdate = DateTime.Now
                    };
                    _context.StockFilial.Add(stock);
                }

                stock.Quantity += quantity;
                stock.LastUpdate = DateTime.Now;

                var movement = new CoffinMovement
                {
                    CoffinVariantId = variantId,
                    BranchId = branchId,
                    Quantity = quantity,
                    Type = "INGRESO_MANUAL",
                    Reference = reference,
                    BalanceAfter = stock.Quantity,
                    RegisteredBy = User.Identity?.Name
                };

                _context.MovimientosAtaudes.Add(movement);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
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
