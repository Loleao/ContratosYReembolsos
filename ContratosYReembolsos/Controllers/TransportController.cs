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
                .Where(c => !_context.VehiculosServicios.Any(vs => vs.ContractId == c.Id && vs.TripStatus == "EN_RUTA"))
                .OrderByDescending(c => c.CreatedAt)
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
            var contrato = await _context.Contratos.FindAsync(contractId);
            if (contrato == null) return NotFound();

            // Importante: Pasar el modelo para que la vista tenga el CemeteryName y otros datos
            ViewBag.ContractId = contractId;
            ViewBag.ContractInfo = $"Contrato #{contrato.ContractNumber} - {contrato.SolicitorName}";

            // REVISA ESTO: Que las listas no estén vacías o nulas
            ViewBag.AvailableVehicles = await _context.Vehiculos
                .Where(v => v.CurrentStatus == "DISPONIBLE" && v.IsActive)
                .ToListAsync() ?? new List<Vehicle>(); // Evita nulos

            ViewBag.AvailableDrivers = await _context.Conductores
                .Where(d => d.IsAvailable && d.IsActive)
                .ToListAsync() ?? new List<Driver>(); // Evita nulos

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
        public async Task<IActionResult> StartTrip(int contractId, int vehicleId, int driverId, string observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var vehicle = await _context.Vehiculos.FindAsync(vehicleId);
                var driver = await _context.Conductores.FindAsync(driverId);

                // Validaciones de seguridad
                if (vehicle == null || vehicle.CurrentStatus != "DISPONIBLE")
                    return Json(new { success = false, message = "El vehículo seleccionado ya no está disponible." });

                if (driver == null || !driver.IsAvailable)
                    return Json(new { success = false, message = "El conductor seleccionado ya no está disponible." });

                // 1. Crear el registro del viaje vinculado al Contrato
                var trip = new VehicleService // Asegúrate que tu modelo tenga ContractId
                {
                    ContractId = contractId,
                    VehicleId = vehicleId,
                    DriverId = driverId,
                    DepartureTime = DateTime.Now,
                    TripStatus = "EN_RUTA",
                    Observations = observations
                };

                // 2. Bloquear recursos (Cambio de estado)
                vehicle.CurrentStatus = "EN_SERVICIO";
                driver.IsAvailable = false;

                _context.VehiculosServicios.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Despacho exitoso. La unidad está en ruta." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error técnico: " + ex.Message });
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
