using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class TransportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard Principal de Movilidad
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingContracts()
        {
            var contracts = await _context.Contratos
                .Include(c => c.MovilityDetails)
                .Where(c => c.MovilityDetails.Any(m => !m.IsDispatched))
                .OrderBy(c => c.BurialDate)
                .ToListAsync();

            return PartialView("Partials/_PendingContracts", contracts);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _context.Vehiculos.ToListAsync();
            return PartialView("Partials/_Vehicles", vehicles);
        }

        [HttpGet]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await _context.Conductores.ToListAsync();
            return PartialView("Partials/_Drivers", drivers);
        }


        [HttpGet]
        public async Task<IActionResult> GetDispatchVehicle(int contractId)
        {
            var contrato = await _context.Contratos
                .Include(c => c.MovilityDetails)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contrato == null) return NotFound();

            ViewBag.PendingServices = contrato.MovilityDetails
                .Where(m => !m.IsDispatched)
                .ToList();

            ViewBag.ContractId = contractId;
            ViewBag.AvailableVehicles = await _context.Vehiculos.Where(v => v.CurrentStatus == "DISPONIBLE").ToListAsync();
            ViewBag.AvailableDrivers = await _context.Conductores.Where(d => d.IsAvailable).ToListAsync();

            return PartialView("Partials/_DispatchVehicle", contrato);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _context.VehiculosServicios
                .Include(s => s.Vehicle)
                .Include(s => s.Driver)
                .Include(s => s.Contract)
                .OrderByDescending(s => s.DepartureTime)
                .ToListAsync();

            return PartialView("Partials/_History", history);
        }

        [HttpPost]
        public async Task<IActionResult> StartTrip(int contractId, int vehicleId, int driverId, string serviceType, string observations)
        {
            if (string.IsNullOrEmpty(serviceType))
            {
                return Json(new { success = false, message = "El tipo de servicio es obligatorio." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Registrar el viaje en el historial
                var trip = new VehicleService
                {
                    ContractId = contractId,
                    VehicleId = vehicleId,
                    DriverId = driverId,
                    ServiceType = serviceType,
                    DepartureTime = DateTime.Now,
                    TripStatus = "EN_RUTA",
                    Observations = observations
                };

                // 2. BUSCAR EL DETALLE ESPECÍFICO Y MARCARLO COMO DESPACHADO
                var detalle = await _context.DetallesMovilidadContrato
                    .FirstOrDefaultAsync(d => d.ContractId == contractId &&
                                             d.ServiceType == serviceType &&
                                             !d.IsDispatched);

                if (detalle != null)
                {
                    detalle.IsDispatched = true;
                }

                // 3. Bloquear Vehículo y Conductor
                var v = await _context.Vehiculos.FindAsync(vehicleId);
                var d = await _context.Conductores.FindAsync(driverId);
                v.CurrentStatus = "EN_SERVICIO";
                d.IsAvailable = false;

                _context.VehiculosServicios.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = $"Unidad {v.Plate} enviada para {serviceType}." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EndTrip(int vehicleId, string observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trip = await _context.VehiculosServicios
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .FirstOrDefaultAsync(t => t.VehicleId == vehicleId && t.TripStatus == "EN_RUTA");

                if (trip == null)
                    return Json(new { success = false, message = "No hay un viaje activo registrado para esta unidad." });

                // 1. Actualizar el registro del servicio
                trip.ReturnTime = DateTime.Now;
                trip.TripStatus = "FINALIZADO";
                trip.Observations = string.IsNullOrEmpty(trip.Observations)
                                    ? observations
                                    : $"{trip.Observations} | Retorno: {observations}";

                // 2. Liberar Vehículo y Conductor
                trip.Vehicle.CurrentStatus = "DISPONIBLE";
                trip.Driver.IsAvailable = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Unidad liberada y lista para el siguiente servicio." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateVehicle()
        {
            return PartialView("Partials/_CreateVehicle");
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.CurrentStatus = "DISPONIBLE";
                vehicle.IsActive = true;
                _context.Vehiculos.Add(vehicle);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Vehículo registrado correctamente." });
            }
            return Json(new { success = false, message = "Datos inválidos." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateDriver()
        {
            return PartialView("Partials/_CreateDriver");
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver(Driver driver)
        {
            if (ModelState.IsValid)
            {
                driver.IsAvailable = true;
                driver.IsActive = true;
                _context.Conductores.Add(driver);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Conductor registrado correctamente." });
            }
            return Json(new { success = false, message = "Datos inválidos." });
        }

    }
}
