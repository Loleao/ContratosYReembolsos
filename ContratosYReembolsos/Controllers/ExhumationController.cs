using ContratosYReembolsos.Services.Interfaces;
using ContratosYReembolsos.Services.DTOs.Exhumations;
using Microsoft.AspNetCore.Mvc;

namespace ContratosYReembolsos.Controllers
{
    public class ExhumationController : Controller
    {
        private readonly IExhumationService _exhumationService;

        public ExhumationController(IExhumationService exhumationService)
        {
            _exhumationService = exhumationService;
        }

        public IActionResult Create()
        {
            return View(new ExhumationCreateDto());
        }

        [HttpGet]
        public IActionResult GetStep(int step)
        {
            var model = new ExhumationCreateDto(); // Aquí podrías persistir data temporal si quieres
            return step switch
            {
                1 => PartialView("Partials/_ExhumationStep1", model),
                2 => PartialView("Partials/_ExhumationStep2", model),
                3 => PartialView("Partials/_ExhumationStep3", model),
                _ => BadRequest()
            };
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExhumationCreateDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _exhumationService.CreateExhumationAsync(model);

            if (result.success)
            {
                // Podrías redirigir al detalle de la exhumación o al Index
                return RedirectToAction("Index", "Contract");
            }

            ModelState.AddModelError("", result.message);
            return View(model);
        }

    }
}