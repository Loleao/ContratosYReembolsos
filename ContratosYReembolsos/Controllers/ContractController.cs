using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Rotativa.AspNetCore;

namespace ContratosYReembolsos.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;

        public ContractController(ApplicationDbContext context, LimaContractsDbContext limaContext)
        {
            _context = context;
            _limaContext = limaContext;
        }

        // 1. Carga la página principal (el cascarón del Stepper)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Función que devuelve el HTML del Paso 1
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
            var depts = await _limaContext.Ubigeos
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
            var provs = await _limaContext.Ubigeos
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
            var dists = await _limaContext.Ubigeos
                .Where(u => u.Region == region && u.Province == province)
                .Select(u => new {
                    inei = u.INEI,
                    distrito = u.District
                })
                .OrderBy(d => d.distrito)
                .ToListAsync();
            return Json(dists);
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
                .Where(c => c.UbigeoId == inei)
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    ruc = c.RUC
                })
                .OrderBy(c => c.name)
                .ToListAsync();

            return Json(cemeteries);
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
        public async Task<IActionResult> GetAgencies(string ruc, string name)
        {
            var query = _context.Agencias.AsQueryable();

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
                .Take(20) // Limitamos para mejorar rendimiento
                .ToListAsync();

            return Json(data);
        }

        public IActionResult GetNicheSelector()
        {
            return PartialView("Partials/_NicheSelector");
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
        public async Task<IActionResult> Create([FromBody] ContractUploadViewModel model)
        {
            if (model == null) return BadRequest(new { message = "Datos no recibidos correctamente." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener Prefijo Regional con validación
                string regionName = "LIMA";
                if (!string.IsNullOrEmpty(model.Deceased.UbigeoFull) && model.Deceased.UbigeoFull.Contains("-"))
                {
                    regionName = model.Deceased.UbigeoFull.Split('-')[0].Trim();
                }

                string prefix = GetRegionPrefix(regionName);
                int currentYear = DateTime.Now.Year;

                // 2. Calcular Correlativo Regional
                int regionCount = await _context.Contratos
                    .CountAsync(c => c.ContractNumber.StartsWith(prefix) && c.CreatedAt.Year == currentYear);
                string contractCode = $"{prefix}{currentYear}{(regionCount + 1).ToString("D5")}";

                // 3. Mapear Cabecera
                var contract = new Contract
                {
                    ContractNumber = contractCode,
                    CreatedAt = DateTime.Now,
                    Status = "Finalizado",
                    SolicitorDni = model.Solicitor.Dni,
                    SolicitorName = model.Solicitor.Name,
                    SolicitorCip = model.Solicitor.Cip,
                    SolicitorType = model.Solicitor.Type,
                    DeceasedDni = model.Deceased.Dni,
                    DeceasedName = model.Deceased.Name,
                    DeathDate = model.Deceased.DeathDate,
                    BurialDate = model.Deceased.BurialDate,

                    // Usamos TryParse para que no explote si la hora viene mal
                    BurialTime = TimeSpan.TryParse(model.Deceased.BurialTime, out var bt) ? bt : TimeSpan.Zero,

                    IneiCode = model.Deceased.Inei,
                    UbigeoFull = model.Deceased.UbigeoFull,
                    WakeId = model.Deceased.WakeId,
                    WakeName = model.Deceased.WakeName,
                    CemeteryId = model.Deceased.CemeteryId,
                    CemeteryName = model.Deceased.CemeteryName,
                    BurialType = model.Deceased.BurialType,
                    BurialDetail = model.Deceased.BurialDetail,
                    AgencyId = model.Agency.Id,
                    AgencyName = model.Agency.Name,
                    AgencyAddress = model.Agency.Address,
                    TotalAmount = model.TotalAmount
                };

                _context.Contratos.Add(contract);
                await _context.SaveChangesAsync(); // Genera el ID

                // 4. Mapear Detalles
                foreach (var s in model.Services)
                {
                    var detail = new ContractDetail
                    {
                        ContractId = contract.Id,
                        ServiceId = s.ServiceId,
                        Observations = $"Lógica: {s.LogicType}",
                        ScheduledDate = (s.LogicType == "MOVILIDAD" && !string.IsNullOrEmpty(s.ExtraData?.ScheduledDate))
                                        ? DateTime.Parse(s.ExtraData.ScheduledDate) : (DateTime?)null,
                        ScheduledTime = (s.LogicType == "MOVILIDAD" && !string.IsNullOrEmpty(s.ExtraData?.ScheduledTime))
                                        ? TimeSpan.Parse(s.ExtraData.ScheduledTime) : (TimeSpan?)null,
                        StockItemId = s.ExtraData?.StockItemId
                    };

                    if (s.LogicType == "ATAUD" && s.ExtraData?.StockItemId != null)
                    {
                        var stock = await _context.StockItems.FindAsync(s.ExtraData.StockItemId);
                        if (stock != null) stock.Status = "Vendido";
                    }

                    _context.DetallesContrato.Add(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // --- EL CAMBIO ESTÁ AQUÍ: Agregamos "id = contract.Id" ---
                return Ok(new
                {
                    success = true,
                    id = contract.Id,
                    contractNumber = contract.ContractNumber
                });
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