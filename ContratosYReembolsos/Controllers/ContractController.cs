using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

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
    }
}