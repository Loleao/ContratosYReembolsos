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
        private readonly LimaContractsDbContext _limaContext;

        public ContractController(LimaContractsDbContext limaContext)
        {
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
    }
}