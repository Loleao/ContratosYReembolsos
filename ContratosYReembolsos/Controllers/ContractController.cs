//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ContratosYReembolsos.Models;
//using Rotativa.AspNetCore;
//using ContratosYReembolsos.Models.ViewModels;
//using Microsoft.AspNetCore.Identity;
//using ContratosYReembolsos.Data.Contexts;
//using ContratosYReembolsos.Models.Entities.Contracts;

//namespace ContratosYReembolsos.Controllers
//{
//    public class ContractController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly LimaContractsDbContext _limaContext;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public ContractController(ApplicationDbContext context, LimaContractsDbContext limaContext, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _limaContext = limaContext;
//            _userManager = userManager;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var userId = _userManager.GetUserId(User);
//            var user = await _context.Users.FindAsync(userId);

//            ViewBag.UserBranchId = user?.BranchId ?? 0;

//            return View();
//        }

//        [HttpGet]
//        public IActionResult GetStep1()
//        {
//            try
//            {
//                return PartialView("Partials/_ContractStep1");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public IActionResult GetStep2()
//        {
//            try
//            {
//                return PartialView("Partials/_ContractStep2");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public IActionResult GetStep3()
//        {
//            try
//            {
//                return PartialView("Partials/_ContractStep3");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public IActionResult GetStep4()
//        {
//            try
//            {
//                return PartialView("Partials/_ContractStep4");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public IActionResult GetStep5()
//        {
//            try
//            {
//                return PartialView("Partials/_ContractStep5");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public IActionResult GetSearchModal()
//        {
//            try
//            {
//                return PartialView("Partials/_SearchAffiliate");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> SearchAffiliates(string dni, string cip, string name)
//        {
//            try
//            {
//                var query = _limaContext.Afiliados.AsQueryable();

//                if (!string.IsNullOrWhiteSpace(dni))
//                    query = query.Where(a => a.DNI.Contains(dni));

//                if (!string.IsNullOrWhiteSpace(cip))
//                    query = query.Where(a => a.CIP.Contains(cip));

//                if (!string.IsNullOrWhiteSpace(name))
//                    query = query.Where(a => a.Name.Contains(name));

//                // Validación: Al menos un filtro debe estar presente
//                if (string.IsNullOrWhiteSpace(dni) && string.IsNullOrWhiteSpace(cip) && string.IsNullOrWhiteSpace(name))
//                    return BadRequest(new { message = "Ingrese al menos un criterio de búsqueda." });

//                var results = await query
//                    .OrderBy(a => a.Name)
//                    .Take(20)
//                    .ToListAsync();

//                return Ok(results);
//            }
//            catch (Exception ex)
//            {
//                // Esto captura el error interno que VS no te muestra detalladamente
//                var mensajeCompleto = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

//                // Imprime en la consola de VS (Output)
//                System.Diagnostics.Trace.WriteLine(">>>> ERROR LIMA DB: " + mensajeCompleto);

//                return StatusCode(500, new
//                {
//                    message = "Error de Mapeo o Conexión",
//                    details = mensajeCompleto
//                });
//            }
//        }

//        [HttpGet]
//        public IActionResult GetSearchBeneficiary()
//         {
//            try
//            {
//                return PartialView("Partials/_SearchBeneficiary");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetBeneficiariesByAffiliate(string afiliadoId)
//        {
//            var beneficiarios = await _limaContext.Beneficiarios
//                .Where(b => b.idfaf == afiliadoId)
//                .Select(b => new {
//                    id = b.codBenef,
//                    dni = "999999999",
//                    name = b.Name,
//                    relationship = b.codParent
//                })
//                .ToListAsync();

//            return Json(beneficiarios);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetRegions()
//        {
//            var depts = await _context.Ubigeos
//                .Select(u => u.Region)
//                .Distinct()
//                .Where(r => r != null)
//                .OrderBy(r => r)
//                .ToListAsync();
//            return Json(depts);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetProvinces(string region)
//        {
//            var provs = await _context.Ubigeos
//                .Where(u => u.Region == region)
//                .Select(u => u.Province)
//                .Distinct()
//                .Where(p => p != null)
//                .OrderBy(p => p)
//                .ToListAsync();
//            return Json(provs);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDistricts(string region, string province)
//        {
//            var dists = await _context.Ubigeos
//                .Where(u => u.Region == region && u.Province == province)
//                .Select(u => new {
//                    inei = u.Id,
//                    distrito = u.District
//                })
//                .OrderBy(d => d.distrito)
//                .ToListAsync();
//            return Json(dists);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetBranchCapabilities(int branchId)
//        {
//            var branch = await _context.Filiales.FindAsync(branchId);
//            if (branch == null) return NotFound();

//            return Json(new
//            {
//                hasWake = branch.HasWakeService,
//                hasCem = branch.HasOwnCemetery,
//                branchName = branch.Name
//            });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetWakes()
//        {
//            var wakes = await _limaContext.Velatorios
//                .Select(w => new { id = w.Id, name = w.Name })
//                .OrderBy(w => w.name)
//                .ToListAsync();
//            return Json(wakes);
//        }

//        [HttpGet]
//        public IActionResult GetSearchCemetery()
//        {
//            try
//            {
//                return PartialView("Partials/_SearchCemetery");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetCemeteries(string? inei, int? branchId)
//        {
//            try
//            {
//                var query = _context.Cementerios.Where(c => c.IsActive);

//                // Si viene INEI, filtramos por ubicación distrital
//                if (!string.IsNullOrEmpty(inei))
//                {
//                    query = query.Where(c => c.UbigeoId == inei);
//                }
//                // Si no hay INEI pero hay BranchId, mostramos los de esa sede
//                else if (branchId.HasValue && branchId > 0)
//                {
//                    query = query.Where(c => c.BranchId == branchId);
//                }

//                var cemeteries = await query
//                    .Select(c => new {
//                        id = c.Id,
//                        name = c.Name,
//                        ruc = c.RUC,
//                        branchId = c.BranchId
//                    })
//                    .ToListAsync();

//                return Json(cemeteries);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error: {ex.Message}");
//            }
//        }

//        public IActionResult GetNicheSelector()
//        {
//            return PartialView("Partials/_NicheSelector");
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetStructures(int cemeteryId, string type)
//        {
//            var structures = await _context.SepulturasEstructura
//                .Where(p => p.CemeteryId == cemeteryId && p.Type == type)
//                .Select(p => new { id = p.Id, name = p.Name })
//                .OrderBy(p => p.name)
//                .ToListAsync();

//            return Json(structures);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetSpaceMap(int structureId)
//        {
//            var spaces = await _context.SepulturasNichos
//                .Where(n => n.StructureId == structureId)
//                .Select(n => new {
//                    id = n.Id,
//                    rowLetter = n.RowLetter,
//                    columnNumber = n.ColumnNumber,
//                    status = n.Status
//                })
//                .OrderBy(n => n.rowLetter).ThenBy(n => n.columnNumber)
//                .ToListAsync();

//            return Json(spaces);
//        }

//        [HttpGet]
//        public IActionResult GetSearchAgency()
//        {
//            try
//            {
//                return PartialView("Partials/_SearchAgency");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error al cargar el componente visual: {ex.Message}");
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAgencies(string ruc, string name, int? branchId)
//        {
//            if (branchId == null || branchId == 0)
//            {
//                return Json(new List<object>()); // Retorna lista vacía si no hay filial
//            }

//            var query = _context.Agencias
//                .Where(a => a.BranchId == branchId && a.IsActive)
//                .AsQueryable();

//            // Log para ver qué está buscando (puedes verlo en el Output de VS)
//            System.Diagnostics.Trace.WriteLine($"Buscando agencias para Filial ID: {branchId}");

//            if (!string.IsNullOrEmpty(ruc))
//                query = query.Where(a => a.RUC.Contains(ruc));

//            if (!string.IsNullOrEmpty(name))
//                query = query.Where(a => a.Name.Contains(name));

//            var data = await query
//                .OrderBy(a => a.Name)
//                .Select(a => new {
//                    id = a.Id,
//                    ruc = a.RUC,
//                    name = a.Name,
//                    address = a.Address,
//                    phone = a.Phone
//                })
//                .Take(20)
//                .ToListAsync();

//            return Json(data);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetCoffinsByBranch(int branchId)
//        {
//            // Solo traemos ataúdes que tengan stock > 0 en esa filial específica
//            var stock = await _context.StockFilial
//                .Include(s => s.CoffinVariant)
//                .ThenInclude(v => v.Coffin)
//                .Where(s => s.BranchId == branchId && s.Quantity > 0)
//                .Select(s => new {
//                    id = s.CoffinVariantId,
//                    name = $"{s.CoffinVariant.Coffin.ModelName} - {s.CoffinVariant.Color}",
//                    stock = s.Quantity,
//                    image = s.CoffinVariant.ImageUrl
//                })
//                .ToListAsync();

//            return Json(stock);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAvailableVehicleTypesByBranch(int branchId)
//        {
//            // Obtenemos los tipos de vehículos que realmente existen en esta filial
//            var availableTypes = await _context.Vehiculos
//                .Include(v => v.VehicleType)
//                .Where(v => v.BranchId == branchId && v.IsActive)
//                .Select(v => new {
//                    id = v.VehicleType.Id,
//                    name = v.VehicleType.Name,
//                    icon = v.VehicleType.Icon
//                })
//                .Distinct()
//                .ToListAsync();

//            return Json(availableTypes);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetBranchPrefix(int branchId)
//        {
//            // Usamos la misma lógica que el Create para ser consistentes
//            var branch = await _context.Filiales
//                .FirstOrDefaultAsync(f => f.Id == branchId);

//            if (string.IsNullOrEmpty(branch?.UbigeoId))
//                return Json(new { prefix = "GEN" });

//            string deptCode = branch.UbigeoId.Substring(0, 2);

//            // Buscamos el registro que tenga la abreviatura para ese departamento
//            var ubigeo = await _context.Ubigeos
//                .Where(u => u.Id.StartsWith(deptCode) && !string.IsNullOrEmpty(u.Abbreviation))
//                .OrderBy(u => u.Id)
//                .FirstOrDefaultAsync();

//            return Json(new { prefix = ubigeo?.Abbreviation ?? "GEN" });
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] ContractViewModel model)
//        {
//            if (model == null) return BadRequest("Datos del contrato no recibidos.");

//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                // 1. Obtener Prefijo Real (LIM, ARE, etc.)
//                string prefix = await GetBranchAbbreviation(model.BranchId);
//                int year = DateTime.Now.Year;

//                // 2. Correlativo Independiente por Prefijo
//                // Contamos cuántos contratos hay en el año que EMPIECEN con ese prefijo
//                int count = await _context.Contratos
//                    .CountAsync(c => c.ContractNumber.StartsWith(prefix) && c.CreatedAt.Year == year) + 1;

//                string contractNumber = $"{prefix}{year}-{count.ToString("D5")}";

//                // 3. Mapeo al Modelo Contract
//                var contract = new Contract
//                {
//                    ContractNumber = contractNumber,
//                    CreatedAt = DateTime.Now,
//                    Status = "ACTIVO",
//                    BranchId = model.BranchId,

//                    SolicitorDni = model.Solicitor.Dni,
//                    SolicitorName = model.Solicitor.Name,
//                    SolicitorType = model.Solicitor.Type,

//                    DeceasedDni = model.Deceased.Dni,
//                    DeceasedName = model.Deceased.Name,
//                    DeathDate = model.Deceased.DeathDate,
//                    BurialDate = model.Deceased.BurialDate,
//                    BurialTime = TimeSpan.Parse(model.Deceased.BurialTime),
//                    UbigeoId = model.Deceased.Inei,

//                    WakeId = model.Deceased.WakeId > 0 ? model.Deceased.WakeId : null,
//                    CemeteryId = model.Deceased.CemeteryId,
//                    IntermentStructureId = model.Deceased.StructureId > 0 ? model.Deceased.StructureId : null,
//                    IntermentSpaceId = model.Deceased.IntermentSpaceId > 0 ? model.Deceased.IntermentSpaceId : null,

//                    CoffinVariantId = model.CoffinVariantId,
//                    AgencyId = model.AgencyId,
//                    TotalAmount = model.TotalAmount
//                };

//                _context.Contratos.Add(contract);
//                await _context.SaveChangesAsync();

//                // 4. Guardar Movilidades (ContractMovilityDetail)
//                if (model.RequiredVehicles != null && model.RequiredVehicles.Any())
//                {
//                    foreach (var vTypeId in model.RequiredVehicles)
//                    {
//                        // Asegúrate que el DbSet se llame ContractMovilityDetails
//                        _context.DetallesMovilidadContrato.Add(new ContractMovilityDetail
//                        {
//                            ContractId = contract.Id,
//                            VehicleTypeId = vTypeId,
//                            Status = "PENDIENTE"
//                        });
//                    }
//                }

//                // 5. Actualizar Estado del Nicho (Si aplica)
//                if (contract.IntermentSpaceId.HasValue)
//                {
//                    var space = await _context.SepulturasNichos.FindAsync(contract.IntermentSpaceId);
//                    if (space != null)
//                    {
//                        // Actualizamos el estado y vinculamos el contrato al nicho si tienes el campo
//                        space.Status = "OCUPADO";
//                        space.ContractId = contract.Id; // Opcional según tu modelo
//                        _context.Update(space);
//                    }
//                }

//                // 6. Descontar Stock (Usando el DbSet correcto de tu migración)
//                var stock = await _context.StockFilial.FirstOrDefaultAsync(s =>
//                    s.BranchId == model.BranchId && s.CoffinVariantId == model.CoffinVariantId);

//                if (stock == null)
//                    throw new Exception("No se encontró registro de inventario para este ataúd en la sede.");

//                if (stock.Quantity <= 0)
//                    throw new Exception("No hay stock disponible del ataúd seleccionado.");

//                stock.Quantity--;
//                _context.Update(stock);

//                await _context.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return Json(new { success = true, id = contract.Id, contractNumber = contractNumber });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                // Log del error para depuración
//                System.Diagnostics.Debug.WriteLine($"ERROR CREATE: {ex.Message}");
//                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
//            }
//        }

//        private async Task<string> GetBranchAbbreviation(int branchId)
//        {
//            // 1. Cargamos la sede y su UbigeoId
//            var branch = await _context.Filiales
//                .FirstOrDefaultAsync(f => f.Id == branchId);

//            if (string.IsNullOrEmpty(branch?.UbigeoId)) return "GEN";

//            // 2. Obtenemos el prefijo del departamento (primeros 2 caracteres)
//            // Ejemplo: "150101" -> "15"
//            string deptCode = branch.UbigeoId.Substring(0, 2);

//            // 3. Buscamos en la tabla Ubigeos el registro que represente al Departamento
//            // Normalmente el departamento tiene Province y District vacíos o nulos
//            var ubigeoDept = await _context.Ubigeos
//                .Where(u => u.Id.StartsWith(deptCode) &&
//                           (u.Province == null || u.Province == ""))
//                .FirstOrDefaultAsync();

//            // 4. Si no tiene abreviatura el padre, intentamos buscar el primero que sí tenga del mismo grupo
//            if (ubigeoDept == null || string.IsNullOrEmpty(ubigeoDept.Abbreviation))
//            {
//                ubigeoDept = await _context.Ubigeos
//                    .Where(u => u.Id.StartsWith(deptCode) && u.Abbreviation != null && u.Abbreviation != "")
//                    .FirstOrDefaultAsync();
//            }

//            return ubigeoDept?.Abbreviation?.ToUpper() ?? "GEN";
//        }

//        [HttpGet]
//        public async Task<IActionResult> DescargarContratoPDF(int id)
//        {
//            var contrato = await _context.Contratos
//                .Include(c => c.Branch)
//                .Include(c => c.CoffinVariant).ThenInclude(v => v.Coffin)
//                .Include(c => c.Cemetery)
//                .Include(c => c.MovilityDetails).ThenInclude(m => m.VehicleType)
//                .FirstOrDefaultAsync(c => c.Id == id);

//            if (contrato == null) return NotFound();

//            return new ViewAsPdf("ContractPrint", contrato)
//            {
//                FileName = $"Contrato_{contrato.ContractNumber}.pdf",
//                PageSize = Rotativa.AspNetCore.Options.Size.A4,
//                CustomSwitches = "--print-media-type --no-outline"
//            };
//        }


//    }
//}

using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace ContratosYReembolsos.Controllers
{
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContractController(IContractService contractService, UserManager<ApplicationUser> userManager)
        {
            _contractService = contractService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserBranchId = user?.BranchId ?? 0;
            return View();
        }

        // Acciones de Pasos (UI Only)
        [HttpGet] public IActionResult GetStep1() => PartialView("Partials/_ContractStep1");
        [HttpGet] public IActionResult GetStep2() => PartialView("Partials/_ContractStep2");
        [HttpGet] public IActionResult GetStep3() => PartialView("Partials/_ContractStep3");
        [HttpGet] public IActionResult GetStep4() => PartialView("Partials/_ContractStep4");
        [HttpGet] public IActionResult GetStep5() => PartialView("Partials/_ContractStep5");
        [HttpGet] public IActionResult GetSearchModal() => PartialView("Partials/_SearchAffiliate");
        [HttpGet] public IActionResult GetSearchBeneficiary() => PartialView("Partials/_SearchBeneficiary");
        [HttpGet] public IActionResult GetSearchCemetery() => PartialView("Partials/_SearchCemetery");
        [HttpGet] public IActionResult GetSearchAgency() => PartialView("Partials/_SearchAgency");
        [HttpGet] public IActionResult GetNicheSelector() => PartialView("Partials/_NicheSelector");

        // APIs de Datos
        [HttpGet] public async Task<IActionResult> SearchAffiliates(string dni, string cip, string name) => Ok(await _contractService.SearchAffiliates(dni, cip, name));
        [HttpGet] public async Task<IActionResult> GetBeneficiariesByAffiliate(string afiliadoId) => Json(await _contractService.GetBeneficiariesByAffiliate(afiliadoId));
        [HttpGet] public async Task<IActionResult> GetRegions() => Json(await _contractService.GetRegions());
        [HttpGet] public async Task<IActionResult> GetProvinces(string region) => Json(await _contractService.GetProvinces(region));
        [HttpGet] public async Task<IActionResult> GetDistricts(string region, string province) => Json(await _contractService.GetDistricts(region, province));
        [HttpGet] public async Task<IActionResult> GetWakes() => Json(await _contractService.GetWakes());
        [HttpGet] public async Task<IActionResult> GetCemeteries(string? inei, int? branchId) => Json(await _contractService.GetCemeteries(inei, branchId));
        [HttpGet] public async Task<IActionResult> GetStructures(int cemeteryId, string type) => Json(await _contractService.GetStructures(cemeteryId, type));
        [HttpGet] public async Task<IActionResult> GetSpaceMap(int structureId) => Json(await _contractService.GetSpaceMap(structureId));
        [HttpGet] public async Task<IActionResult> GetAgencies(string ruc, string name, int? branchId) => Json(await _contractService.GetAgencies(ruc, name, branchId));
        [HttpGet] public async Task<IActionResult> GetCoffinsByBranch(int branchId) => Json(await _contractService.GetCoffinsByBranch(branchId));
        [HttpGet] public async Task<IActionResult> GetAvailableVehicleTypesByBranch(int branchId) => Json(await _contractService.GetAvailableVehicleTypesByBranch(branchId));
        [HttpGet] public async Task<IActionResult> GetBranchPrefix(int branchId) => Json(new { prefix = await _contractService.GetBranchAbbreviation(branchId) });

        [HttpGet]
        public async Task<IActionResult> GetBranchCapabilities(int branchId)
        {
            // Si el branchId es 0 o negativo, devolvemos un objeto básico para evitar errores en JS
            if (branchId <= 0)
            {
                return Json(new { hasWake = false, hasCem = false, branchName = "No seleccionada" });
            }

            var capabilities = await _contractService.GetBranchCapabilities(branchId);

            if (capabilities == null) return NotFound();

            return Json(capabilities);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractViewModel model)
        {
            if (model == null) return BadRequest("Datos inválidos");
            var res = await _contractService.CreateContract(model);
            return Json(new { success = res.success, message = res.message, id = res.contractId, contractNumber = res.contractNumber });
        }

        [HttpGet]
        public async Task<IActionResult> DescargarContratoPDF(int id)
        {
            var contrato = await _contractService.GetContractForPdf(id);
            if (contrato == null) return NotFound();
            return new ViewAsPdf("ContractPrint", contrato) { FileName = $"Contrato_{contrato.ContractNumber}.pdf" };
        }
    }
}