//using ContratosYReembolsos.Data.Contexts;
//using ContratosYReembolsos.Models;
//using ContratosYReembolsos.Models.Entities.Transport;
//using ContratosYReembolsos.Models.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.CodeAnalysis.Operations;
//using Microsoft.EntityFrameworkCore;

//namespace ContratosYReembolsos.Controllers
//{
//    public class TransportController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public TransportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // Dashboard Principal de Movilidad
//        public async Task<IActionResult> Index(int? branchId)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return Challenge();

//            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
//            int targetBranchId;

//            if (isAdmin)
//            {
//                // Si el admin no ha seleccionado filial, cargamos la vista de selección
//                if (branchId == null)
//                {
//                    // Obtenemos las filiales con sus relaciones para evitar errores de carga
//                    var branches = await _context.Filiales
//                        .Include(b => b.Ubigeo)
//                        .Select(b => new {
//                            Id = b.Id,
//                            Name = b.Name,
//                            City = b.Ubigeo != null ? b.Ubigeo.Region : b.Address,
//                            VehicleCount = _context.Vehiculos.Count(v => v.BranchId == b.Id),
//                            PendingContracts = _context.Contratos.Count(c => c.BranchId == b.Id && c.Status == "Pendiente")
//                        }).ToListAsync();

//                    // IMPORTANTE: Pasamos 'branches' como el MODELO de la vista
//                    return View("BranchSelection", branches);
//                }
//                targetBranchId = branchId.Value;
//            }
//            else
//            {
//                if (user.BranchId == null) return Content("Su usuario no tiene una filial asignada.");
//                targetBranchId = user.BranchId.Value;
//            }

//            // Guardamos info en ViewBag para la vista operativa
//            ViewBag.IsAdmin = isAdmin;
//            ViewBag.SelectedBranchId = targetBranchId;

//            var branch = await _context.Filiales.FindAsync(targetBranchId);
//            ViewBag.BranchName = branch?.Name ?? "Sede Desconocida";

//            ViewBag.PendingCount = await _context.Contratos
//                .CountAsync(c => c.BranchId == targetBranchId && c.Status == "Pendiente");

//            return View();
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetPendingContracts()
//        {
//            var contracts = await _context.Contratos
//                .Include(c => c.MovilityDetails)
//                    .ThenInclude(m => m.VehicleType)
//                .Where(c => c.MovilityDetails.Any(detail =>
//                    !_context.VehiculosServicios.Any(vs => vs.ContractMovilityDetailId == detail.Id)))
//                .OrderBy(c => c.BurialDate)
//                .ThenBy(c => c.BurialTime)
//                .Select(c => new PendingContractViewModel
//                {
//                    Id = c.Id,
//                    ContractNumber = c.ContractNumber,
//                    DeceasedName = c.DeceasedName,
//                    BurialDate = c.BurialDate,
//                    BurialTime = c.BurialTime,
//                    // Filtramos aquí mismo los detalles que no tienen servicio registrado
//                    PendingDetails = c.MovilityDetails
//                        .Where(detail => !_context.VehiculosServicios.Any(vs => vs.ContractMovilityDetailId == detail.Id))
//                        .ToList()
//                })
//                .ToListAsync();

//            return PartialView("Partials/_PendingContracts", contracts);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetVehicles()
//        {
//            var vehicles = await _context.Vehiculos.ToListAsync();
//            return PartialView("Partials/_Vehicles", vehicles);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDrivers()
//        {
//            var drivers = await _context.Conductores.ToListAsync();
//            return PartialView("Partials/_Drivers", drivers);
//        }


//        [HttpGet]
//        public async Task<IActionResult> GetDispatchVehicle(int contractId, int detailId)
//        {
//            // 1. Buscamos el contrato con el detalle específico que se quiere despachar
//            var contrato = await _context.Contratos
//                .Include(c => c.MovilityDetails)
//                    .ThenInclude(m => m.VehicleType)
//                .FirstOrDefaultAsync(c => c.Id == contractId);

//            if (contrato == null) return NotFound();

//            // 2. Buscamos el detalle específico para validar que no esté ya en ruta
//            var detail = contrato.MovilityDetails.FirstOrDefault(m => m.Id == detailId);

//            // Verificamos si ya existe un servicio activo o finalizado para este detalle
//            var yaDespachado = await _context.VehiculosServicios
//                .AnyAsync(vs => vs.ContractMovilityDetailId == detailId && vs.TripStatus != "CANCELADO");

//            if (yaDespachado)
//                return BadRequest("Este servicio de movilidad ya ha sido despachado o está en ruta.");

//            // 3. Pasamos la información necesaria a la vista mediante ViewBag
//            ViewBag.TargetDetail = detail; // El servicio específico: "Carroza", "Bus", etc.
//            ViewBag.ContractId = contractId;

//            // Solo vehículos disponibles y conductores libres
//            ViewBag.AvailableVehicles = await _context.Vehiculos
//                .Where(v => v.CurrentStatus == "DISPONIBLE" && v.IsActive)
//                .ToListAsync();

//            ViewBag.AvailableDrivers = await _context.Conductores
//                .Where(d => d.IsAvailable && d.IsActive)
//                .ToListAsync();

//            return PartialView("Partials/_DispatchVehicle", contrato);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetHistory()
//        {
//            var history = await _context.VehiculosServicios
//                .Include(s => s.Vehicle)
//                .Include(s => s.Driver)
//                .Include(s => s.Contract)
//                .OrderByDescending(s => s.DepartureTime)
//                .ToListAsync();

//            return PartialView("Partials/_History", history);
//        }

//        [HttpPost]
//        public async Task<IActionResult> StartTrip(int contractId, int contractMovilityDetailId, int vehicleId, int driverId, string observations)
//        {
//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                // 1. Validar disponibilidad de Vehículo y Conductor (Doble verificación por seguridad)
//                var v = await _context.Vehiculos.FindAsync(vehicleId);
//                var d = await _context.Conductores.FindAsync(driverId);

//                if (v == null || v.CurrentStatus != "DISPONIBLE")
//                    return Json(new { success = false, message = "El vehículo ya no está disponible." });

//                if (d == null || !d.IsAvailable)
//                    return Json(new { success = false, message = "El conductor ya no está disponible." });

//                // 2. Obtener el detalle del contrato para vincularlo
//                var detalle = await _context.DetallesMovilidadContrato
//                    .Include(m => m.VehicleType)
//                    .FirstOrDefaultAsync(m => m.Id == contractMovilityDetailId);

//                if (detalle == null)
//                    return Json(new { success = false, message = "El requerimiento de movilidad no existe." });

//                // 3. Crear el registro de ejecución (VehicleService)
//                var trip = new VehicleService
//                {
//                    ContractId = contractId,
//                    ContractMovilityDetailId = contractMovilityDetailId, // LA LLAVE MAESTRA
//                    VehicleId = vehicleId,
//                    DriverId = driverId,
//                    DepartureTime = DateTime.Now,
//                    TripStatus = "EN_RUTA",
//                    Observations = observations
//                };

//                // 4. Actualizar estados de los activos (Bloqueo)
//                v.CurrentStatus = "EN_SERVICIO";
//                d.IsAvailable = false;

//                // 5. Guardar cambios
//                _context.VehiculosServicios.Add(trip);
//                await _context.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return Json(new
//                {
//                    success = true,
//                    message = $"Unidad {v.Plate} enviada con éxito para el servicio de {detalle.VehicleType.Name}."
//                });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return Json(new { success = false, message = "Error interno: " + ex.Message });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> EndTrip(int vehicleId, string observations)
//        {
//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                var trip = await _context.VehiculosServicios
//                    .Include(t => t.Vehicle)
//                    .Include(t => t.Driver)
//                    .FirstOrDefaultAsync(t => t.VehicleId == vehicleId && t.TripStatus == "EN_RUTA");

//                if (trip == null)
//                    return Json(new { success = false, message = "No hay un viaje activo registrado para esta unidad." });

//                // 1. Actualizar el registro del servicio
//                trip.ReturnTime = DateTime.Now;
//                trip.TripStatus = "FINALIZADO";
//                trip.Observations = string.IsNullOrEmpty(trip.Observations)
//                                    ? observations
//                                    : $"{trip.Observations} | Retorno: {observations}";

//                // 2. Liberar Vehículo y Conductor
//                trip.Vehicle.CurrentStatus = "DISPONIBLE";
//                trip.Driver.IsAvailable = true;

//                await _context.SaveChangesAsync();
//                await transaction.CommitAsync();

//                return Json(new { success = true, message = "Unidad liberada y lista para el siguiente servicio." });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return Json(new { success = false, message = "Error: " + ex.Message });
//            }
//        }

//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> VehicleTypes()
//        {
//            var types = await _context.TiposVehiculo
//                .OrderBy(t => t.Name)
//                .ToListAsync();

//            return View(types);
//        }

//        [HttpGet]
//        public IActionResult GetCreateVehicleType()
//        {
//            return PartialView("Partials/_CreateVehicleType");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateVehicleType(VehicleType model)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.TiposVehiculo.Add(model);
//                await _context.SaveChangesAsync();
//                return Json(new { success = true, message = "Tipo de vehículo creado correctamente." });
//            }
//            return Json(new { success = false, message = "Datos inválidos." });
//        }

//        [HttpGet]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> GetEditVehicleType(int id)
//        {
//            var type = await _context.TiposVehiculo.FindAsync(id);
//            if (type == null) return NotFound();
//            return PartialView("Partials/_EditVehicleType", type);
//        }

//        [HttpPost]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> EditVehicleType(VehicleType model)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Update(model);
//                await _context.SaveChangesAsync();
//                return Json(new { success = true, message = "Tipo de vehículo actualizado correctamente." });
//            }
//            return Json(new { success = false, message = "Datos del formulario inválidos." });
//        }

//        // POST: Alternar Activo/Inactivo
//        [HttpPost]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> ToggleVehicleTypeStatus(int id)
//        {
//            var type = await _context.TiposVehiculo.FindAsync(id);
//            if (type == null) return Json(new { success = false, message = "Tipo no encontrado." });

//            type.IsActive = !type.IsActive;
//            await _context.SaveChangesAsync();

//            string status = type.IsActive ? "activado" : "desactivado";
//            return Json(new { success = true, message = $"El tipo ha sido {status} correctamente." });
//        }

//        // POST: Eliminar lógicamente o físicamente
//        [HttpPost]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> DeleteVehicleType(int id)
//        {
//            var type = await _context.TiposVehiculo.FindAsync(id);
//            if (type == null) return Json(new { success = false, message = "Tipo no encontrado." });

//            // Verificación de integridad: No borrar si hay vehículos de este tipo
//            bool hasVehicles = await _context.Vehiculos.AnyAsync(v => v.VehicleTypeId == id);
//            if (hasVehicles)
//            {
//                return Json(new
//                {
//                    success = false,
//                    message = "No se puede eliminar este tipo porque existen vehículos registrados con esta categoría. Considere desactivarlo en su lugar."
//                });
//            }

//            _context.TiposVehiculo.Remove(type);
//            await _context.SaveChangesAsync();
//            return Json(new { success = true, message = "Tipo de vehículo eliminado definitivamente." });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetCreateVehicle(int? branchId)
//        {
//            if (branchId == null)
//            {
//                return BadRequest("El ID de la filial es requerido.");
//            }

//            var branch = await _context.Filiales.FindAsync(branchId);
//            if (branch == null)
//            {
//                return NotFound("La filial especificada no existe.");
//            }

//            // Aseguramos que cargamos la lista de tipos activos
//            var vehicleTypes = await _context.TiposVehiculo
//                .Where(t => t.IsActive)
//                .OrderBy(t => t.Name)
//                .ToListAsync();

//            // Pasamos la data de forma segura a la vista parcial
//            ViewBag.VehicleTypes = vehicleTypes;
//            ViewBag.BranchName = branch.Name;
//            ViewBag.BranchId = branchId;

//            return PartialView("Partials/_CreateVehicle");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
//        {
//            if (ModelState.IsValid)
//            {
//                vehicle.CurrentStatus = "DISPONIBLE";
//                vehicle.IsActive = true;
//                _context.Vehiculos.Add(vehicle);
//                await _context.SaveChangesAsync();
//                return Json(new { success = true, message = "Vehículo registrado correctamente." });
//            }
//            return Json(new { success = false, message = "Datos inválidos." });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetCreateDriver(int? branchId)
//        {
//            if (branchId == null || branchId == 0)
//            {
//                return BadRequest("El ID de la filial es necesario para registrar un conductor.");
//            }

//            var branch = await _context.Filiales.FindAsync(branchId);

//            if (branch == null)
//            {
//                return NotFound("La filial especificada no existe.");
//            }

//            ViewBag.BranchName = branch.Name;
//            ViewBag.BranchId = branch.Id;

//            // 4. Retornamos la vista parcial
//            return PartialView("Partials/_CreateDriver");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateDriver(Driver model)
//        {
//            // Eliminamos la navegación virtual para que no invalide el modelo
//            ModelState.Remove("Branch");
//            ModelState.Remove("CurrentStatus");

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    model.IsActive = true;
//                    model.IsAvailable = true;

//                    _context.Conductores.Add(model);
//                    await _context.SaveChangesAsync();

//                    return Json(new { success = true, message = "Conductor registrado correctamente." });
//                }
//                catch (Exception ex)
//                {
//                    return Json(new { success = false, message = "Error al guardar: " + ex.Message });
//                }
//            }

//            var errorMsg = string.Join(" | ", ModelState.Values
//                .SelectMany(v => v.Errors)
//                .Select(e => e.ErrorMessage));

//            return Json(new { success = false, message = "Datos inválidos: " + errorMsg });
//        }

//    }
//}


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

        public async Task<IActionResult> Index(int? branchId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            int targetBranchId;

            if (isAdmin)
            {
                if (branchId == null) return View("BranchSelection", await _transportService.GetBranchesSelectionData());
                targetBranchId = branchId.Value;
            }
            else
            {
                if (user.BranchId == null) return Content("Sin filial asignada.");
                targetBranchId = user.BranchId.Value;
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.SelectedBranchId = targetBranchId;
            var branch = await _context.Filiales.FindAsync(targetBranchId);
            ViewBag.BranchName = branch?.Name;
            ViewBag.PendingCount = await _context.Contratos.CountAsync(c => c.BranchId == targetBranchId && c.Status == "Pendiente");

            return View();
        }

        [HttpGet] public async Task<IActionResult> GetPendingContracts() => PartialView("Partials/_PendingContracts", await _transportService.GetPendingContracts());
        [HttpGet] public async Task<IActionResult> GetVehicles() => PartialView("Partials/_Vehicles", await _transportService.GetVehicles());
        [HttpGet] public async Task<IActionResult> GetDrivers() => PartialView("Partials/_Drivers", await _transportService.GetDrivers());
        [HttpGet] public async Task<IActionResult> GetHistory() => PartialView("Partials/_History", await _transportService.GetHistory());

        [HttpGet]
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
        public async Task<IActionResult> StartTrip(int contractId, int contractMovilityDetailId, int vehicleId, int driverId, string observations)
        {
            var result = await _transportService.StartTrip(contractId, contractMovilityDetailId, vehicleId, driverId, observations);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> EndTrip(int vehicleId, string observations)
        {
            var result = await _transportService.EndTrip(vehicleId, observations);
            return Json(new { success = result.success, message = result.message });
        }

        [Authorize(Roles = "Admin")] public async Task<IActionResult> VehicleTypes() => View(await _transportService.GetVehicleTypes());
        [HttpGet] public IActionResult GetCreateVehicleType() => PartialView("Partials/_CreateVehicleType");
        
        [HttpPost]
        public async Task<IActionResult> CreateVehicleType(VehicleType model)
        {
            var result = await _transportService.CreateVehicleType(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet] public async Task<IActionResult> GetEditVehicleType(int id) => PartialView("Partials/_EditVehicleType", await _context.TiposVehiculo.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> EditVehicleType(VehicleType model)
        {
            var result = await _transportService.EditVehicleType(model);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVehicleTypeStatus(int id)
        {
            var result = await _transportService.ToggleVehicleTypeStatus(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicleType(int id)
        {
            var result = await _transportService.DeleteVehicleType(id);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateVehicle(int? branchId)
        {
            ViewBag.VehicleTypes = await _context.TiposVehiculo.Where(t => t.IsActive).ToListAsync();
            ViewBag.BranchId = branchId;
            return PartialView("Partials/_CreateVehicle");
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            var result = await _transportService.CreateVehicle(vehicle);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateDriver(int? branchId)
        {
            ViewBag.BranchId = branchId;
            return PartialView("Partials/_CreateDriver");
        }

        [HttpPost]
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