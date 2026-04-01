using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.External;
using ContratosYReembolsos.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    //public class CoffinController : Controller
    //{
    //    private readonly ApplicationDbContext _context;
    //    private readonly IWebHostEnvironment _hostEnvironment;

    //    public CoffinController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    //    {
    //        _context = context;
    //        _hostEnvironment = hostEnvironment;
    //    }

    //    // GET: Coffins (Listado tipo Catálogo)
    //    public async Task<IActionResult> Index()
    //    {
    //        // Obtenemos los ataúdes de la base de datos
    //        var data = await _context.Ataudes.ToListAsync();
    //        return View(data);
    //    }

    //    // GET: Coffins/Create
    //    public IActionResult Create() => View();

    //    // POST: Coffins/Create
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Create(Coffin coffin, IFormFile? imageFile)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            // Lógica para guardar la imagen si existe
    //            if (imageFile != null)
    //            {
    //                string wwwRootPath = _hostEnvironment.WebRootPath;
    //                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
    //                string path = Path.Combine(wwwRootPath, "images", "coffins");

    //                // Crear carpeta si no existe
    //                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

    //                using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
    //                {
    //                    await imageFile.CopyToAsync(fileStream);
    //                }
    //                coffin.ImageUrl = "/images/coffins/" + fileName;
    //            }

    //            _context.Add(coffin);
    //            await _context.SaveChangesAsync();
    //            return RedirectToAction(nameof(Index));
    //        }
    //        return View(coffin);
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> RegisterEntry(int coffinId, int quantity, string reference)
    //    {
    //        var coffin = await _context.Ataudes.FindAsync(coffinId);
    //        if (coffin == null) return NotFound();

    //        using var transaction = await _context.Database.BeginTransactionAsync();
    //        try
    //        {
    //            coffin.CurrentStock += quantity;

    //            // 2. Registrar el movimiento en el Kardex
    //            var movement = new CoffinMovement
    //            {
    //                CoffinId = coffinId,
    //                Quantity = quantity,
    //                Type = "INGRESO",
    //                Reference = reference,
    //                Date = DateTime.Now
    //            };

    //            _context.MovimientosAtaudes.Add(movement);
    //            await _context.SaveChangesAsync();
    //            await transaction.CommitAsync();

    //            return Json(new { success = true, newStock = coffin.CurrentStock });
    //        }
    //        catch (Exception ex)
    //        {
    //            await transaction.RollbackAsync();
    //            return Json(new { success = false, message = ex.Message });
    //        }
    //    }

    //    // POST: Coffins/UpdateStock (Para ajustes rápidos de inventario)
    //    [HttpPost]
    //    public async Task<IActionResult> UpdateStock(int id, int amount)
    //    {
    //        var coffin = await _context.Ataudes.FindAsync(id);
    //        if (coffin == null) return NotFound();

    //        coffin.CurrentStock += amount;
    //        await _context.SaveChangesAsync();

    //        return Json(new { success = true, newStock = coffin.CurrentStock });
    //    }

    //    // GET: Coffins/Delete/5
    //    public async Task<IActionResult> Delete(int id)
    //    {
    //        var coffin = await _context.Ataudes.FindAsync(id);
    //        if (coffin != null)
    //        {
    //            // Borrar imagen física si existe
    //            if (!string.IsNullOrEmpty(coffin.ImageUrl))
    //            {
    //                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, coffin.ImageUrl.TrimStart('/'));
    //                if (System.IO.File.Exists(imagePath)) System.IO.File.Exists(imagePath);
    //            }

    //            _context.Ataudes.Remove(coffin);
    //            await _context.SaveChangesAsync();
    //        }
    //        return RedirectToAction(nameof(Index));
    //    }
    //}

    public class CoffinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;

        public CoffinController(ApplicationDbContext context, LimaContractsDbContext limaContext)
        {
            _context = context;
            _limaContext = limaContext;
        }

        // PASO 1: Mostrar todas las Filiales (codfilial, descripcion...)
        public async Task<IActionResult> Index(string searchCode, string searchName, int page = 1)
        {
            int pageSize = 8; // Número de filiales por página
            IQueryable<Subsidiary> query = _limaContext.Filiales.AsQueryable();

            // Filtro por Código
            if (!string.IsNullOrEmpty(searchCode))
            {
                query = query.Where(f => f.Id.Contains(searchCode));
            }

            // Filtro por Nombre
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(f => f.Name.Contains(searchName));
            }

            // Paginación
            var totalItems = await query.CountAsync();
            var filiales = await query
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Datos para la vista
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchName = searchName;

            return View(filiales);
        }

        [HttpGet]
        public async Task<IActionResult> GetFilialesGrid(string searchCode, string searchName, int page = 1)
        {
            int pageSize = 8;
            IQueryable<Subsidiary> query = _limaContext.Filiales.AsQueryable();

            if (!string.IsNullOrEmpty(searchCode))
                query = query.Where(f => f.Id.Contains(searchCode));

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(f => f.Name.Contains(searchName));

            var totalItems = await query.CountAsync();
            var filiales = await query
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Datos para que la parcial sepa en qué página está
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchCode = searchCode;
            ViewBag.SearchName = searchName;

            return PartialView("Partials/_SubsidiaryStock", filiales);
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
            const string CENTRAL_ID = "070000";

            // Traemos variantes que tengan cantidad > 0 en la Central
            ViewBag.Products = await _context.StockFilial
                .Include(s => s.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(s => s.SubsidiaryId == CENTRAL_ID && s.Quantity > 0)
                .Select(s => new {
                    VariantId = s.CoffinVariantId,
                    DisplayName = $"{s.CoffinVariant.Coffin.ModelName} - {s.CoffinVariant.Color} ({s.CoffinVariant.Material})",
                    StockDisponible = s.Quantity
                }).ToListAsync();

            // Traemos todas las filiales destino menos la central
            ViewBag.Subsidiaries = await _limaContext.Filiales
                .Where(f => f.Id != CENTRAL_ID)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return PartialView("Partials/_CreateTransferStock");
        }

        public async Task<IActionResult> Inventory(string id)
        {
            var subsidiary = await _limaContext.Filiales.FirstOrDefaultAsync(f => f.Id == id);
            if (subsidiary == null) return NotFound();

            ViewBag.SubsidiaryId = id;
            ViewBag.SubsidiaryName = subsidiary.Name;
            return View(); // Retorna la vista marco
        }

        // 2. Parcial: Stock Real de la Filial
        [HttpGet]
        public async Task<IActionResult> GetFilialStock(string id)
        {
            var inventory = await _context.StockFilial
                .Include(s => s.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(s => s.SubsidiaryId == id)
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
        public async Task<IActionResult> GetFilialPendingTransfers(string id)
        {
            var pending = await _context.AtaudTransferencias
                .Include(t => t.CoffinVariant).ThenInclude(v => v.Coffin)
                .Where(t => t.TargetSubsidiaryId == id && t.Status == "EN_CAMINO")
                .AsNoTracking().ToListAsync();

            ViewBag.SubsidiaryId = id;
            return PartialView("Partials/_FilialPendingTransfers", pending);
        }

        [HttpPost]
        public async Task<IActionResult> TransferFromGeneral(int variantId, string targetSubsidiaryId, int quantity, string reference)
        {
            const string CENTRAL_ID = "070000";
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validar Stock en Origen
                var centralStock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == variantId && s.SubsidiaryId == CENTRAL_ID);

                if (centralStock == null || centralStock.Quantity < quantity)
                    return Json(new { success = false, message = "Stock insuficiente en Central." });

                // 2. Ejecutar Salida Física
                centralStock.Quantity -= quantity;
                centralStock.LastUpdate = DateTime.Now;

                // 3. Crear el Movimiento de Salida (Kardex)
                var moveOut = new CoffinMovement
                {
                    CoffinVariantId = variantId,
                    SubsidiaryId = CENTRAL_ID,
                    Quantity = -quantity, // Negativo porque sale
                    Type = "TRANSFERENCIA_OUT",
                    Reference = reference,
                    Date = DateTime.Now,
                    BalanceAfter = centralStock.Quantity,
                    RegisteredBy = User.Identity?.Name ?? "Admin"
                };
                _context.MovimientosAtaudes.Add(moveOut);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id de moveOut

                // 4. Crear la Transferencia Logística (Tu idea de los 2 campos)
                var transfer = new CoffinTransfer
                {
                    CoffinVariantId = variantId,
                    OriginSubsidiaryId = CENTRAL_ID,
                    TargetSubsidiaryId = targetSubsidiaryId,
                    Quantity = quantity,
                    Status = "EN_CAMINO",
                    GuiaRemision = reference,
                    DateSent = DateTime.Now,
                    SentBy = User.Identity?.Name ?? "Admin",
                    DepartureMovementId = moveOut.Id // <-- Puntero al movimiento que inició todo
                };
                _context.AtaudTransferencias.Add(transfer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Envío procesado. Stock en tránsito." });
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
                // 1. Obtener la transferencia pendiente
                var transfer = await _context.AtaudTransferencias.FindAsync(transferId);
                if (transfer == null || transfer.Status != "EN_CAMINO")
                    return Json(new { success = false, message = "Transferencia no encontrada o ya procesada." });

                // 2. Buscar o crear el Stock en la Filial Destino
                var targetStock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == transfer.CoffinVariantId && s.SubsidiaryId == transfer.TargetSubsidiaryId);

                if (targetStock == null)
                {
                    targetStock = new BranchStock
                    {
                        CoffinVariantId = transfer.CoffinVariantId,
                        SubsidiaryId = transfer.TargetSubsidiaryId,
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
                    SubsidiaryId = transfer.TargetSubsidiaryId,
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
        public async Task<IActionResult> GetAddStock(int variantId, string subsidiaryId)
        {
            var variant = await _context.AtaudVariantes
                .Include(v => v.Coffin)
                .FirstOrDefaultAsync(v => v.Id == variantId);

            var filial = await _limaContext.Filiales.FirstOrDefaultAsync(f => f.Id == subsidiaryId);

            if (variant == null || filial == null) return NotFound();

            ViewBag.VariantId = variant.Id;
            ViewBag.ModelName = $"{variant.Coffin.ModelName} ({variant.Color})";
            ViewBag.SubsidiaryName = filial.Name;
            ViewBag.SubsidiaryId = subsidiaryId;

            return PartialView("Partials/_AddStock");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterEntry(int variantId, string subsidiaryId, int quantity, string reference)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscar o Crear el registro de Stock para esa filial
                var stock = await _context.StockFilial
                    .FirstOrDefaultAsync(s => s.CoffinVariantId == variantId && s.SubsidiaryId == subsidiaryId);

                if (stock == null)
                {
                    stock = new BranchStock
                    {
                        CoffinVariantId = variantId,
                        SubsidiaryId = subsidiaryId,
                        Quantity = 0,
                        MinimumStock = 5, // Valor por defecto
                        LastUpdate = DateTime.Now
                    };
                    _context.StockFilial.Add(stock);
                }

                // 2. Actualizar Cantidad
                stock.Quantity += quantity;
                stock.LastUpdate = DateTime.Now;

                // 3. Registrar el Movimiento en el Kardex (Auditoría)
                var movement = new CoffinMovement
                {
                    CoffinVariantId = variantId,
                    SubsidiaryId = subsidiaryId,
                    Quantity = quantity,
                    Type = "INGRESO_MANUAL", // Diferenciar de Transferencias
                    Reference = reference,
                    Date = DateTime.Now,
                    BalanceAfter = stock.Quantity,
                    RegisteredBy = User.Identity?.Name ?? "SedeUser"
                };

                _context.MovimientosAtaudes.Add(movement);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Ingreso local registrado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error al registrar: " + ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create(CoffinCreateViewModel vm, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(vm);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear el Modelo Base
                var newCoffin = new Coffin { ModelName = vm.ModelName };
                _context.Ataudes.Add(newCoffin);
                await _context.SaveChangesAsync();

                // 2. Manejo de Imagen
                string imageUrl = "/images/coffins/default.jpg";
                if (imageFile != null) { /* Tu lógica de guardado de archivo aquí... */ }

                // 3. Crear la Variante Inicial
                var variant = new CoffinVariant
                {
                    CoffinModelId = newCoffin.Id,
                    Color = vm.Color,
                    Material = vm.Material,
                    Size = vm.Size,
                    ImageUrl = imageUrl
                };
                _context.AtaudVariantes.Add(variant);
                await _context.SaveChangesAsync();

                // 4. Asignar Stock Inicial a la Central (070000)
                var initialStock = new BranchStock
                {
                    CoffinVariantId = variant.Id,
                    SubsidiaryId = "070000",
                    Quantity = vm.InitialStock,
                    MinimumStock = vm.MinimumStock
                };
                _context.StockFilial.Add(initialStock);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                await transaction.RollbackAsync();
                return View(vm);
            }
        }
    }
}
