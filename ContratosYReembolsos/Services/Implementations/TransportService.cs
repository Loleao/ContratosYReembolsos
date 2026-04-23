using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Transport;
using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Transport
{
    public class TransportService : ITransportService
    {
        private readonly ApplicationDbContext _context;

        public TransportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetBranchesSelectionData()
        {
            return await _context.Filiales
                .Include(b => b.Ubigeo)
                .Select(b => new {
                    Id = b.Id,
                    Name = b.Name,
                    City = b.Ubigeo != null ? b.Ubigeo.Region : b.Address,
                    VehicleCount = _context.Vehiculos.Count(v => v.BranchId == b.Id),
                    PendingContracts = _context.Contratos.Count(c => c.BranchId == b.Id && c.Status == "Pendiente")
                }).Cast<object>().ToListAsync();
        }

        public async Task<List<PendingContractViewModel>> GetPendingContracts()
        {
            return await _context.Contratos
                .Include(c => c.MovilityDetails).ThenInclude(m => m.VehicleType)
                .Where(c => c.MovilityDetails.Any(detail => !_context.VehiculosServicios.Any(vs => vs.ContractMovilityDetailId == detail.Id)))
                .OrderBy(c => c.BurialDate).ThenBy(c => c.BurialTime)
                .Select(c => new PendingContractViewModel
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    DeceasedName = c.DeceasedName,
                    BurialDate = c.BurialDate,
                    BurialTime = c.BurialTime,
                    PendingDetails = c.MovilityDetails.Where(detail => !_context.VehiculosServicios.Any(vs => vs.ContractMovilityDetailId == detail.Id)).ToList()
                }).ToListAsync();
        }

        public async Task<List<Vehicle>> GetVehicles() => await _context.Vehiculos.ToListAsync();
        public async Task<List<Driver>> GetDrivers() => await _context.Conductores.ToListAsync();
        public async Task<List<VehicleType>> GetVehicleTypes() => await _context.TiposVehiculo.OrderBy(t => t.Name).ToListAsync();
        public async Task<List<VehicleService>> GetHistory() => await _context.VehiculosServicios.Include(s => s.Vehicle).Include(s => s.Driver).Include(s => s.Contract).OrderByDescending(s => s.DepartureTime).ToListAsync();

        public async Task<(bool success, string message)> StartTrip(int contractId, int contractMovilityDetailId, int vehicleId, int driverId, string observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var v = await _context.Vehiculos.FindAsync(vehicleId);
                var d = await _context.Conductores.FindAsync(driverId);
                if (v == null || v.CurrentStatus != "DISPONIBLE") return (false, "Vehículo no disponible.");
                if (d == null || !d.IsAvailable) return (false, "Conductor no disponible.");

                var trip = new VehicleService
                {
                    ContractId = contractId,
                    ContractMovilityDetailId = contractMovilityDetailId,
                    VehicleId = vehicleId,
                    DriverId = driverId,
                    DepartureTime = DateTime.Now,
                    TripStatus = "EN_RUTA",
                    Observations = observations
                };

                v.CurrentStatus = "EN_SERVICIO";
                d.IsAvailable = false;
                _context.VehiculosServicios.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Viaje iniciado");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> EndTrip(int vehicleId, string observations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trip = await _context.VehiculosServicios.Include(t => t.Vehicle).Include(t => t.Driver)
                    .FirstOrDefaultAsync(t => t.VehicleId == vehicleId && t.TripStatus == "EN_RUTA");
                if (trip == null) return (false, "No hay viaje activo.");

                trip.ReturnTime = DateTime.Now;
                trip.TripStatus = "FINALIZADO";
                trip.Observations = string.IsNullOrEmpty(trip.Observations) ? observations : $"{trip.Observations} | Retorno: {observations}";
                trip.Vehicle.CurrentStatus = "DISPONIBLE";
                trip.Driver.IsAvailable = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Unidad liberada");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> CreateVehicleType(VehicleType model)
        {
            _context.TiposVehiculo.Add(model); await _context.SaveChangesAsync(); return (true, "Creado");
        }

        public async Task<(bool success, string message)> EditVehicleType(VehicleType model)
        {
            _context.Update(model); await _context.SaveChangesAsync(); return (true, "Actualizado");
        }

        public async Task<(bool success, string message)> ToggleVehicleTypeStatus(int id)
        {
            var type = await _context.TiposVehiculo.FindAsync(id);
            if (type == null) return (false, "No existe");
            type.IsActive = !type.IsActive; await _context.SaveChangesAsync();
            return (true, "Estado cambiado");
        }

        public async Task<(bool success, string message)> DeleteVehicleType(int id)
        {
            if (await _context.Vehiculos.AnyAsync(v => v.VehicleTypeId == id)) return (false, "Tiene vehículos asociados");
            var type = await _context.TiposVehiculo.FindAsync(id);
            _context.TiposVehiculo.Remove(type); await _context.SaveChangesAsync();
            return (true, "Eliminado");
        }

        public async Task<(bool success, string message)> CreateVehicle(Vehicle vehicle)
        {
            vehicle.CurrentStatus = "DISPONIBLE"; vehicle.IsActive = true;
            _context.Vehiculos.Add(vehicle); await _context.SaveChangesAsync(); return (true, "Registrado");
        }

        public async Task<(bool success, string message)> CreateDriver(Driver driver)
        {
            driver.IsActive = true; driver.IsAvailable = true;
            _context.Conductores.Add(driver); await _context.SaveChangesAsync(); return (true, "Registrado");
        }
    }
}