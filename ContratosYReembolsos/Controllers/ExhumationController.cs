using ContratosYReembolsos.Models.ViewModels.Exhumations;
using ContratosYReembolsos.Services.DTOs.Exhumations;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace ContratosYReembolsos.Controllers
{
    public class ExhumationController : Controller
    {
        private readonly IExhumationService _exhumationService;

        public ExhumationController(IExhumationService exhumationService)
        {
            _exhumationService = exhumationService;
        }

        public IActionResult Index() => View();
        public IActionResult Create() => View();

        [HttpGet]
        public IActionResult GetStep(int step)
        {
            return step switch
            {
                1 => PartialView("Partials/_ExhumationStep1"),
                2 => PartialView("Partials/_ExhumationStep2"),
                3 => PartialView("Partials/_ExhumationStep3"),
                _ => BadRequest()
            };
        }

        [HttpGet]
        public IActionResult GetSearchContractModal() => PartialView("Partials/_SearchContractExhumation");

        [HttpGet]
        public IActionResult GetNicheSelectorModal() => PartialView("Partials/_NicheSelectorExhumation");

        [HttpGet]
        public async Task<IActionResult> SearchContracts(string dni, string name)
            => Json(await _exhumationService.SearchContractsAsync(dni, name));

        [HttpGet]
        public async Task<IActionResult> SearchDeceased(string query)
        {
            var results = await _exhumationService.SearchDeceasedAsync(query);
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ExhumationCreateDto model)
        {
            if (model == null) return BadRequest(new { success = false, message = "Datos nulos" });

            // Capturamos el resultado del servicio
            var result = await _exhumationService.RegisterExhumationAsync(model);

            // Retornamos un objeto con nombres de propiedades claros para el JS
            return Json(new
            {
                success = result.success,
                message = result.message
            });
        }

        [HttpGet]
        public async Task<IActionResult> PrintExhumation(int id)
        {
            var dto = await _exhumationService.GetExhumationForPdfAsync(id);

            if (dto == null) return NotFound();

            // Mapeo de DTO a ViewModel
            var viewModel = new ExhumationPrintViewModel
            {
                Folio = dto.ExhumationNumber,
                FechaEmision = dto.RequestDate.ToString("dd/MM/yyyy HH:mm"),
                NombreFallecido = dto.DeceasedName.ToUpper(),
                Dni = dto.DeceasedDni,
                UbicacionOrigen = dto.OriginLocation,
                UbicacionDestino = dto.DestinationLocation,
                TipoTramite = dto.MovementType,
                MontoTotal = dto.TotalCost.ToString("C2", new System.Globalization.CultureInfo("es-PE")),
                Notas = string.IsNullOrEmpty(dto.Observations) ? "Ninguna" : dto.Observations
            };

            return new ViewAsPdf("ExhumationPrint", viewModel)
            {
                FileName = $"Exhumacion_{dto.ExhumationNumber}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };
        }

    }
}