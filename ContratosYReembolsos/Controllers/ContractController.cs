using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Rotativa.AspNetCore;
using ContratosYReembolsos.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ContratosYReembolsos.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContractController(ApplicationDbContext context, LimaContractsDbContext limaContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _limaContext = limaContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FindAsync(userId);

            ViewBag.UserBranchId = user?.BranchId ?? 0;

            return View();
        }

        [HttpGet]
        public IActionResult GetStep1()
        {
            try
            {
                return PartialView("Partials/_ContractStep1");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetStep2()
        {
            try
            {
                return PartialView("Partials/_ContractStep2");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetStep3()
        {
            try
            {
                return PartialView("Partials/_ContractStep3");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetStep4()
        {
            try
            {
                return PartialView("Partials/_ContractStep4");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetStep5()
        {
            try
            {
                return PartialView("Partials/_ContractStep5");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetSearchModal()
        {
            try
            {
                return PartialView("Partials/_SearchAffiliate");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchAffiliates(string dni, string cip, string name)
        {
            try
            {
                var query = _limaContext.Afiliados.AsQueryable();

                if (!string.IsNullOrWhiteSpace(dni))
                    query = query.Where(a => a.DNI.Contains(dni));

                if (!string.IsNullOrWhiteSpace(cip))
                    query = query.Where(a => a.CIP.Contains(cip));

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(a => a.Name.Contains(name));

                // Validación: Al menos un filtro debe estar presente
                if (string.IsNullOrWhiteSpace(dni) && string.IsNullOrWhiteSpace(cip) && string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "Ingrese al menos un criterio de búsqueda." });

                var results = await query
                    .OrderBy(a => a.Name)
                    .Take(20)
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Esto captura el error interno que VS no te muestra detalladamente
                var mensajeCompleto = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // Imprime en la consola de VS (Output)
                System.Diagnostics.Trace.WriteLine(">>>> ERROR LIMA DB: " + mensajeCompleto);

                return StatusCode(500, new
                {
                    message = "Error de Mapeo o Conexión",
                    details = mensajeCompleto
                });
            }
        }

        [HttpGet]
        public IActionResult GetSearchBeneficiary()
         {
            try
            {
                return PartialView("Partials/_SearchBeneficiary");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBeneficiariesByAffiliate(string afiliadoId)
        {
            var beneficiarios = await _limaContext.Beneficiarios
                .Where(b => b.idfaf == afiliadoId)
                .Select(b => new {
                    id = b.codBenef,
                    dni = "999999999",
                    name = b.Name,
                    relationship = b.codParent
                })
                .ToListAsync();

            return Json(beneficiarios);
        }

        [HttpGet]
        public async Task<IActionResult> GetRegions()
        {
            var depts = await _context.Ubigeos
                .Select(u => u.Region)
                .Distinct()
                .Where(r => r != null)
                .OrderBy(r => r)
                .ToListAsync();
            return Json(depts);
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinces(string region)
        {
            var provs = await _context.Ubigeos
                .Where(u => u.Region == region)
                .Select(u => u.Province)
                .Distinct()
                .Where(p => p != null)
                .OrderBy(p => p)
                .ToListAsync();
            return Json(provs);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(string region, string province)
        {
            var dists = await _context.Ubigeos
                .Where(u => u.Region == region && u.Province == province)
                .Select(u => new {
                    inei = u.Id,
                    distrito = u.District
                })
                .OrderBy(d => d.distrito)
                .ToListAsync();
            return Json(dists);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchCapabilities(int branchId)
        {
            var branch = await _context.Filiales.FindAsync(branchId);
            if (branch == null) return NotFound();

            return Json(new
            {
                hasWake = branch.HasWakeService,
                hasCem = branch.HasOwnCemetery,
                branchName = branch.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWakes()
        {
            var wakes = await _limaContext.Velatorios
                .Select(w => new { id = w.Id, name = w.Name })
                .OrderBy(w => w.name)
                .ToListAsync();
            return Json(wakes);
        }

        [HttpGet]
        public IActionResult GetSearchCemetery()
        {
            try
            {
                return PartialView("Partials/_SearchCemetery");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCemeteries(string inei)
        {
            var cemeteries = await _context.Cementerios
                .Where(c => c.Branch.UbigeoId == inei) // Usamos UbigeoId de tu modelo
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    ruc = c.RUC,
                    branchId = c.BranchId // <-- Crucial: Enviamos el ID de la filial
                })
                .OrderBy(c => c.name)
                .ToListAsync();

            return Json(cemeteries);
        }

        public IActionResult GetNicheSelector()
        {
            return PartialView("Partials/_NicheSelector");
        }

        [HttpGet]
        public async Task<IActionResult> GetStructures(int cemeteryId, string type)
        {
            var structures = await _context.SepulturasEstructura
                .Where(p => p.CemeteryId == cemeteryId && p.Type == type)
                .Select(p => new { id = p.Id, name = p.Name })
                .OrderBy(p => p.name)
                .ToListAsync();

            return Json(structures);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpaceMap(int structureId)
        {
            var spaces = await _context.SepulturasNichos
                .Where(n => n.StructureId == structureId)
                .Select(n => new {
                    id = n.Id,
                    rowLetter = n.RowLetter,
                    columnNumber = n.ColumnNumber,
                    status = n.Status
                })
                .OrderBy(n => n.rowLetter).ThenBy(n => n.columnNumber)
                .ToListAsync();

            return Json(spaces);
        }

        [HttpGet]
        public IActionResult GetSearchAgency()
        {
            try
            {
                return PartialView("Partials/_SearchAgency");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencies(string ruc, string name, int? branchId)
        {
            if (branchId == null || branchId == 0)
            {
                return Json(new List<object>()); // Retorna lista vacía si no hay filial
            }

            var query = _context.Agencias
                .Where(a => a.BranchId == branchId && a.IsActive)
                .AsQueryable();

            // Log para ver qué está buscando (puedes verlo en el Output de VS)
            System.Diagnostics.Trace.WriteLine($"Buscando agencias para Filial ID: {branchId}");

            if (!string.IsNullOrEmpty(ruc))
                query = query.Where(a => a.RUC.Contains(ruc));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(a => a.Name.Contains(name));

            var data = await query
                .OrderBy(a => a.Name)
                .Select(a => new {
                    id = a.Id,
                    ruc = a.RUC,
                    name = a.Name,
                    address = a.Address,
                    phone = a.Phone
                })
                .Take(20)
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCoffinsByBranch(int branchId)
        {
            // Solo traemos ataúdes que tengan stock > 0 en esa filial específica
            var stock = await _context.StockFilial
                .Include(s => s.CoffinVariant)
                .ThenInclude(v => v.Coffin)
                .Where(s => s.BranchId == branchId && s.Quantity > 0)
                .Select(s => new {
                    id = s.CoffinVariantId,
                    name = $"{s.CoffinVariant.Coffin.ModelName} - {s.CoffinVariant.Color}",
                    stock = s.Quantity,
                    image = s.CoffinVariant.ImageUrl
                })
                .ToListAsync();

            return Json(stock);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableVehicleTypesByBranch(int branchId)
        {
            // Obtenemos los tipos de vehículos que realmente existen en esta filial
            var availableTypes = await _context.Vehiculos
                .Include(v => v.VehicleType)
                .Where(v => v.BranchId == branchId && v.IsActive)
                .Select(v => new {
                    id = v.VehicleType.Id,
                    name = v.VehicleType.Name,
                    icon = v.VehicleType.Icon
                })
                .Distinct()
                .ToListAsync();

            return Json(availableTypes);
        }

        private string GetRegionPrefix(string regionName)
        {
            if (string.IsNullOrEmpty(regionName)) return "GEN";

            var prefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Amazonas", "AMA" }, 
                { "Ancash", "ANC" }, 
                { "Apurimac", "APU" },
                { "Arequipa", "ARE" }, 
                { "Ayacucho", "AYA" }, 
                { "Cajamarca", "CAJ" },
                { "Callao", "CAL" }, 
                { "Cusco", "CUS" }, 
                { "Huancavelica", "HUV" },
                { "Huanuco", "HUA" }, 
                { "Ica", "ICA" }, 
                { "Junin", "JUN" },
                { "La Libertad", "LIB" }, 
                { "Lambayeque", "LAM" }, 
                { "Lima", "LIM" },
                { "Loreto", "LOR" }, 
                { "Madre de Dios", "MDD" }, 
                { "Moquegua", "MOQ" },
                { "Pasco", "PAS" }, 
                { "Piura", "PIU" }, 
                { "Puno", "PUN" },
                { "San Martin", "SAM" }, 
                { "Tacna", "TAC" }, 
                { "Tumbes", "TUM" },
                { "Ucayali", "UCA" }
            };

            // Buscamos el nombre de la región. Si no existe, devolvemos los 3 primeros caracteres en mayúsculas.
            if (prefixes.TryGetValue(regionName, out string prefix))
            {
                return prefix;
            }

            return regionName.Length >= 3 ? regionName.Substring(0, 3).ToUpper() : "GEN";
        }

        [HttpGet]
        public IActionResult GetPrefix(string region)
        {
            if (string.IsNullOrEmpty(region)) return Json(new { prefix = "GEN" });

            string prefix = GetRegionPrefix(region);
            return Json(new { prefix });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractViewModel model) // Usamos tu nuevo ViewModel
        {
            if (model == null) return BadRequest(new { message = "Datos no recibidos correctamente." });

            // Obtenemos el usuario para saber su filial
            var user = await _userManager.GetUserAsync(User);
            int branchId = user.BranchId ?? 0;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lógica de Prefijo y Correlativo (Mejorada para usar la filial del usuario)
                var branch = await _context.Filiales.FindAsync(branchId);
                string regionName = branch?.Ubigeo?.Region ?? "LIMA";
                string prefix = GetRegionPrefix(regionName);
                int currentYear = DateTime.Now.Year;

                int regionCount = await _context.Contratos
                    .CountAsync(c => c.ContractNumber.StartsWith(prefix) && c.CreatedAt.Year == currentYear);

                string contractCode = $"{prefix}{currentYear}{(regionCount + 1).ToString("D5")}";

                // 2. Mapear Cabecera con las nuevas relaciones
                var contract = new Contract
                {
                    ContractNumber = contractCode,
                    CreatedAt = DateTime.Now,
                    Status = "Finalizado",
                    BranchId = branchId, // Vínculo con Filial

                    // Datos del Solicitante
                    SolicitorDni = model.SolicitorDni,
                    SolicitorName = model.SolicitorName,
                    SolicitorCip = model.SolicitorCip,
                    SolicitorType = model.SolicitorType,

                    // Datos del Fallecido
                    DeceasedDni = model.DeceasedDni,
                    DeceasedName = model.DeceasedName,
                    DeathDate = model.DeathDate,
                    BurialDate = model.BurialDate,
                    BurialTime = model.BurialTime,
                    UbigeoFull = model.UbigeoFull,

                    // Sepulturas (Relación Real)
                    IntermentSpaceId = model.IntermentSpaceId,
                    BurialDetail = model.BurialDetail,

                    // Agencia (Relación Real)
                    AgencyId = model.AgencyId,

                    TotalAmount = model.TotalAmount
                };

                _context.Contratos.Add(contract);
                await _context.SaveChangesAsync();

                // 3. Actualizar estado del Nicho a "Ocupado"
                if (contract.IntermentSpaceId.HasValue)
                {
                    var space = await _context.SepulturasNichos.FindAsync(contract.IntermentSpaceId);
                    if (space != null) space.Status = "Ocupado";
                }

                // 4. Mapear Ataúd (ContractDetail) y Movilidades (ContractMovilityDetail)
                // Aquí iteramos los detalles que vienen del Wizard...

                // Ejemplo para el Ataúd:
                if (model.SelectedStockItemId > 0)
                {
                    var stock = await _context.StockItems.FindAsync(model.SelectedStockItemId);
                    if (stock != null)
                    {
                        stock.Status = "Vendido";
                        _context.DetallesContrato.Add(new ContractDetail
                        {
                            ContractId = contract.Id,
                            ServiceId = 1, // Supongamos que 1 es 'Ataúd'
                            StockItemId = stock.Id,
                            Price = 0 // O el precio del stock
                        });
                    }
                }

                // Ejemplo para Movilidades:
                foreach (var type in model.SelectedMovilityTypes)
                {
                    _context.DetallesMovilidadContrato.Add(new ContractMovilityDetail
                    {
                        ContractId = contract.Id,
                        ServiceType = type,
                        IsDispatched = false,
                        ScheduledDate = contract.BurialDate
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, id = contract.Id, contractNumber = contract.ContractNumber });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarContratoPDF(int id)
        {
            // Buscamos el contrato con sus detalles y el nombre de los servicios
            var contrato = await _context.Contratos
                .Include(c => c.Details)
                    .ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contrato == null) return NotFound();

            return new ViewAsPdf("ContractPrint", contrato)
            {
                FileName = $"Contrato_{contrato.ContractNumber}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--print-media-type --no-outline"
            };
        }


    }
}