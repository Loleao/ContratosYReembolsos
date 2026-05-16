using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using ContratosYReembolsos.Models.ViewModels.Contracts;
using ContratosYReembolsos.Models.Entities.Contracts;
using Microsoft.AspNetCore.Authorization;
using ContratosYReembolsos.Services.DTOs.Contracts;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Services.Implementations.Cemeteries;

namespace ContratosYReembolsos.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ICurrentUserService _currentUser;
        private readonly IInhumationService _inhumationService; // 👈 NUEVO SERVICIO INYECTADO
        private readonly IAuthorizationService _authorizationService;

        public ContractController(IContractService contractService, ICurrentUserService currentUser, IInhumationService inhumationService, IAuthorizationService authorizationService)
        {
            _contractService = contractService;
            _currentUser = currentUser;
            _inhumationService = inhumationService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Contratos.Ver")]
        public async Task<IActionResult> Index(int? selectedBranchId)
        {
            // Lógica de redirección limpia usando el asistente
            if (_currentUser.IsAdmin && selectedBranchId == null)
            {
                var groupedBranches = await _contractService.GetBranchesGroupedByRegionAsync();
                return View("BranchSelection", groupedBranches);
            }

            var dtos = await _contractService.GetContractListByBranchAsync(selectedBranchId);

            // Mapeo a ViewModel
            var viewModels = dtos.Select(d => new ContractListViewModel
            {
                Id = d.Id,
                ContractNumber = d.ContractNumber,
                DeceasedName = d.DeceasedName,
                FullBurialDetail = $"{d.BurialDate:dd/MM/yyyy} - {d.BurialTime:hh\\:mm}",
                BranchName = d.BranchName,
                Status = d.Status
            }).ToList();

            ViewBag.BranchName = dtos.FirstOrDefault()?.BranchName ?? "Sede seleccionada";
            ViewBag.IsAdmin = _currentUser.IsAdmin;

            return View(viewModels);
        }

        public async Task<IActionResult> PrintContract(int id)
        {
            var model = await _contractService.GetContractForPDFAsync(id);

            if (model == null) return NotFound();

            // 1. Definimos el nombre del archivo de forma dinámica
            string fileName = $"Contrato_{model.ContractNumber.Replace("/", "-")}.pdf";

            // 2. Agregamos el encabezado Content-Disposition
            // 'inline' permite que se abra en el navegador.
            // 'filename' sugiere el nombre al momento de descargar.
            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");

            // 3. Retornamos la vista como PDF sin la propiedad FileName de Rotativa
            return new ViewAsPdf("ContractPrint", model)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(15, 15, 15, 15),
                // Esto también ayuda a que el título de la pestaña del navegador cambie
                CustomSwitches = "--page-offset 0 --footer-center [page] --footer-font-size 8"
            };
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _contractService.GetContractDetailsAsync(id);
            if (dto == null) return NotFound();

            // Aquí llamarías a un nuevo método en el ExhumationService o ContractService 
            // para traer el historial. Por ahora lo simulamos o mapeamos:
            var viewModel = new ContractDetailsViewModel
            {
                Data = dto,
                // Aquí traerías la data real de la tabla Exhumaciones
                MovementHistory = await _contractService.GetMovementHistoryAsync(id)
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Contratos.Crear")]
        public IActionResult Create()
        {
            ViewBag.UserBranchId = _currentUser.BranchId ?? 0;
            return View();
        }

        // Acciones de Pasos (UI Only)
        [HttpGet] public IActionResult GetStep1() => PartialView("Partials/_ContractStep1");
        [HttpGet] public IActionResult GetStep2() => PartialView("Partials/_ContractStep2");
        [HttpGet] public IActionResult GetStep3() => PartialView("Partials/_ContractStep3");
        [HttpGet] public IActionResult GetStep4() => PartialView("Partials/_ContractStep4");
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
        [HttpGet]
        public async Task<IActionResult> GetWakes(int branchId)
        {
            // Si no viene branchId, intentamos sacarlo del usuario actual
            int finalBranchId = branchId > 0 ? branchId : (_currentUser.BranchId ?? 0);
            return Json(await _contractService.GetWakes(finalBranchId));
        }
        [HttpGet] public async Task<IActionResult> GetCemeteries(string? inei, int? branchId) => Json(await _contractService.GetCemeteries(inei, branchId));
        [HttpGet] public async Task<IActionResult> GetStructures(int cemeteryId, string type) => Json(await _contractService.GetStructures(cemeteryId, type));
        [HttpGet] public async Task<IActionResult> GetSpaceMap(int structureId) => Json(await _contractService.GetSpaceMap(structureId));
        [HttpGet] public async Task<IActionResult> GetBranchPrefix(int branchId) => Json(new { prefix = await _contractService.GetBranchAbbreviation(branchId) });
        [HttpGet]
        public async Task<IActionResult> GetAgencies(int branchId)
        {
            // Usamos el servicio que ya tienes implementado
            var agencies = await _contractService.GetAgencies(null, null, branchId);
            return Json(agencies);
        }
        [HttpGet]
        public async Task<IActionResult> GetBranchCapabilities(int branchId)
        {
            // Si el branchId es 0 o negativo, devolvemos un objeto básico para evitar errores en JS
            if (branchId <= 0)
            {
                return Json(new { hasWake = false, branchName = "No seleccionada" });
            }

            var capabilities = await _contractService.GetBranchCapabilities(branchId);

            if (capabilities == null) return NotFound();

            return Json(capabilities);
        }
        public async Task<IActionResult> GetInventoryStock(int branchId)
            => Json(await _contractService.GetStockItemsByBranch(branchId));
        [HttpGet] public async Task<IActionResult> GetAvailableVehicleTypesByBranch(int branchId) => Json(await _contractService.GetAvailableVehicleTypesByBranch(branchId));

        [HttpGet]
        public async Task<IActionResult> GetFuneralServices(int? agencyId)
        {
            // Pasamos el agencyId directamente al método actualizado de tu capa de negocio
            var services = await _contractService.GetFuneralServices(agencyId);
            return Json(services);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Contratos.Crear")]
        public async Task<IActionResult> Create([FromBody] ContractViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Esto te dirá exactamente qué campo no coincide
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }
            var res = await _contractService.CreateContract(model);
            return Json(new { success = res.success, message = res.message, id = res.contractId, contractNumber = res.contractNumber });
        }

        // Añade esta acción dentro de tu ContractController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Contratos.Editar")] // Control de accesos de tu sistema
        public async Task<IActionResult> ConfirmProductDelivery([FromBody] ConfirmProductDeliveryInput model)
        {
            if (model == null || model.Id <= 0)
            {
                return Json(new { success = false, message = "Datos de entrega del producto inválidos." });
            }

            var result = await _contractService.ConfirmProductDeliveryAsync(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Contratos.Editar")]
        public async Task<IActionResult> AdvanceServiceStatus([FromBody] AdvanceServiceStatusInput model)
        {
            if (model == null || model.Id <= 0)
            {
                return Json(new { success = false, message = "Datos de avance del servicio inválidos." });
            }

            var result = await _contractService.AdvanceServiceStatusAsync(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.CatalogoServicios.Ver")] 
        public async Task<IActionResult> FuneralServices()
        {
            var services = await _contractService.GetAllServicesAsync();
            return View(services);
        }

        [HttpGet]
        [Authorize] // Permitimos la entrada general para evaluar adentro
        public async Task<IActionResult> GetFuneralServiceForm(int id = 0)
        {
            // Caso: Creación (id == 0)
            if (id == 0)
            {
                var canCreate = (await _authorizationService.AuthorizeAsync(User, "Permissions.CatalogoServicios.Crear")).Succeeded;
                if (!canCreate) return StatusCode(403);

                return PartialView("Partials/_FuneralServiceForm", new FuneralService());
            }

            // Caso: Edición (id > 0)
            var canEdit = (await _authorizationService.AuthorizeAsync(User, "Permissions.CatalogoServicios.Editar")).Succeeded;
            if (!canEdit) return StatusCode(403);

            var services = await _contractService.GetAllServicesAsync();
            var service = services.FirstOrDefault(s => s.Id == id);

            if (service == null) return NotFound();

            return PartialView("Partials/_FuneralServiceForm", service);
        }

        [HttpPost]
        [Authorize] // Quitamos la política específica de aquí para evaluar el Id adentro
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFuneralService(FuneralService model)
        {
            // 1. Validación Granular de Permisos
            if (model.Id == 0)
            {
                // Si intenta crear, verificamos permiso de Crear
                var canCreate = (await _authorizationService.AuthorizeAsync(User, "Permissions.CatalogoServicios.Crear")).Succeeded;
                if (!canCreate) return Json(new { success = false, message = "No tiene permiso para crear nuevos servicios." });
            }
            else
            {
                // Si intenta editar, verificamos permiso de Editar
                var canEdit = (await _authorizationService.AuthorizeAsync(User, "Permissions.CatalogoServicios.Editar")).Succeeded;
                if (!canEdit) return Json(new { success = false, message = "No tiene permiso para editar servicios existentes." });
            }

            // 2. Lógica de guardado
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos del formulario inválidos." });

            var result = await _contractService.UpsertServiceAsync(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public async Task<IActionResult> GetDirectInhumationForm(int cemeteryId, int? structureId, int? spaceId)
        {
            // 1. Consumimos los nombres técnicos mediante el servicio especialista
            var displayData = await _inhumationService.GetInhumationDisplayNamesAsync(cemeteryId, spaceId);

            // 2. Cargamos los ViewBags con la data segura devuelta
            ViewBag.CemeteryName = displayData.cemeteryName;
            ViewBag.SpaceCode = displayData.spaceCode;

            // 3. Instanciamos el InputModel limpio
            var model = new InhumationWithoutContractInput
            {
                CemeteryId = cemeteryId,
                StructureId = structureId,
                IntermentSpaceId = spaceId,
                BurialDate = DateTime.Now,
                DeathDate = DateTime.Now
            };

            return PartialView("Partials/_DirectInhumationForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public async Task<IActionResult> SaveDirectInhumation([FromBody] InhumationWithoutContractInput model)
        {
            if (model == null) return Json(new { success = false, message = "Petición vacía." });

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = "Verifique campos: " + errors });
            }

            // Consumimos el servicio especializado
            var result = await _inhumationService.RegisterDirectInhumationAsync(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Espacios.Crear")]
        public IActionResult CreateDirectInhumation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetDirectInhumationStep(int step)
        {
            return step switch
            {
                1 => PartialView("Partials/_InhumationStep1"),
                2 => PartialView("Partials/_InhumationStep2"),
                3 => PartialView("Partials/_InhumationStep3"),
                _ => BadRequest()
            };
        }
    }
}