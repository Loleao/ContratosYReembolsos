using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Transport;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    [Authorize]
    public class TransportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITransportService _transportService;

        public TransportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ITransportService transportService)
        {
            _context = context;
            _userManager = userManager;
            _transportService = transportService;
        }


        [Authorize(Policy = "Permissions.Movilidad.Ver")]
        public async Task<IActionResult> Index(int? branchId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            int targetBranchId;

            if (isAdmin)
            {
                // Si es Admin y no ha seleccionado una filial, mostramos el selector regional
                if (branchId == null)
                {
                    var groupedModel = await _transportService.GetBranchesGroupedByRegionAsync();
                    return View("BranchSelection", groupedModel);
                }
                targetBranchId = branchId.Value;
            }
            else
            {
                // Si NO es Admin, forzamos el uso de su filial asignada
                if (user.BranchId == null)
                {
                    return Content("Acceso denegado: Su cuenta no tiene una filial vinculada.");
                }
                targetBranchId = user.BranchId.Value;
            }

            // Configuración de la vista para la filial seleccionada o asignada
            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedBranchId = targetBranchId;

            // Obtenemos el nombre de la filial para el encabezado
            var branch = await _context.Filiales.FindAsync(targetBranchId);
            ViewBag.BranchName = branch?.Name;

            // Contador de contratos pendientes específico de la sede
            ViewBag.PendingCount = await _context.Contratos
                .CountAsync(c => c.BranchId == targetBranchId && c.Status == "Pendiente");

            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Movilidad.Ver")]
        public async Task<IActionResult> GetPendingContracts() => PartialView("Partials/_PendingContracts", await _transportService.GetPendingContracts());
        [HttpGet]
        [Authorize(Policy = "Permissions.Vehiculos.Ver")]
        public async Task<IActionResult> GetVehicles() => PartialView("Partials/_Vehicles", await _transportService.GetVehicles());
        [HttpGet]
        [Authorize(Policy = "Permissions.Conductores.Ver")]
        public async Task<IActionResult> GetDrivers() => PartialView("Partials/_Drivers", await _transportService.GetDrivers());
        [HttpGet]
        [Authorize(Policy = "Permissions.Movilidad.Ver")]
        public async Task<IActionResult> GetHistory() => PartialView("Partials/_History", await _transportService.GetHistory());

        [HttpGet]
        [Authorize(Policy = "Permissions.Movilidad.Asignar")]
        public async Task<IActionResult> GetDispatchVehicle(int contractId, int detailId)
        {
            var contrato = await _context.Contratos.Include(c => c.MovilityDetails).ThenInclude(m => m.VehicleType).FirstOrDefaultAsync(c => c.Id == contractId);
            if (contrato == null) return NotFound();

            if (await _context.VehiculosServicios.AnyAsync(vs => vs.ContractMovilityDetailId == detailId && vs.TripStatus != "CANCELADO"))
                return BadRequest("Ya despachado.");

            ViewBag.TargetDetail = contrato.MovilityDetails.FirstOrDefault(m => m.Id == detailId);
            ViewBag.ContractId = contractId;
            ViewBag.AvailableVehicles = await _context.Vehiculos.Where(v => v.CurrentStatus == "DISPONIBLE" && v.IsActive).ToListAsync();
            ViewBag.AvailableDrivers = await _context.Conductores.Where(d => d.IsAvailable && d.IsActive).ToListAsync();

            return PartialView("Partials/_DispatchVehicle", contrato);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Movilidad.Asignar")]
        public async Task<IActionResult> StartTrip(int contractId, int contractMovilityDetailId, int vehicleId, int driverId, string observations)
        {
            var result = await _transportService.StartTrip(contractId, contractMovilityDetailId, vehicleId, driverId, observations);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Movilidad.Finalizar")]
        public async Task<IActionResult> EndTrip(int vehicleId, string observations)
        {
            var result = await _transportService.EndTrip(vehicleId, observations);
            return Json(new { success = result.success, message = result.message });
        }

        [Authorize(Policy = "Permissions.CatalogoVehiculos.Ver")]
        public async Task<IActionResult> VehicleTypes() => View(await _transportService.GetVehicleTypes());
        [HttpGet]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Crear")]
        public IActionResult GetCreateVehicleType() => PartialView("Partials/_CreateVehicleType");
        
        [HttpPost]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Crear")]
        public async Task<IActionResult> CreateVehicleType(VehicleType model)
        {
            var result = await _transportService.CreateVehicleType(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Editar")]
        public async Task<IActionResult> GetEditVehicleType(int id) => PartialView("Partials/_EditVehicleType", await _context.TiposVehiculo.FindAsync(id));

        [HttpPost]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Editar")]
        public async Task<IActionResult> EditVehicleType(VehicleType model)
        {
            var result = await _transportService.EditVehicleType(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Editar")]
        public async Task<IActionResult> ToggleVehicleTypeStatus(int id)
        {
            var result = await _transportService.ToggleVehicleTypeStatus(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CatalogoVehiculos.Eliminar")]
        public async Task<IActionResult> DeleteVehicleType(int id)
        {
            var result = await _transportService.DeleteVehicleType(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Vehiculos.Crear")]
        public async Task<IActionResult> GetCreateVehicle(int? branchId)
        {
            ViewBag.VehicleTypes = await _context.TiposVehiculo.Where(t => t.IsActive).ToListAsync();
            ViewBag.BranchId = branchId;
            return PartialView("Partials/_CreateVehicle");
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Vehiculos.Crear")]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            var result = await _transportService.CreateVehicle(vehicle);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Conductores.Crear")]
        public async Task<IActionResult> GetCreateDriver(int? branchId)
        {
            ViewBag.BranchId = branchId;
            return PartialView("Partials/_CreateDriver");
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Conductores.Crear")]
        public async Task<IActionResult> CreateDriver(Driver model)
        {
            ModelState.Remove("Branch");
            ModelState.Remove("CurrentStatus");

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos del formulario inválidos." });

            var result = await _transportService.CreateDriver(model);
            return Json(new { success = result.success, message = result.message });
        }
    }
}