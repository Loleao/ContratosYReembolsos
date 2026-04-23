using ContratosYReembolsos.Models.Entities.Transport;
using ContratosYReembolsos.Models.ViewModels;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface ITransportService
    {
        // Consultas de Dashboard y Selección
        Task<List<object>> GetBranchesSelectionData();
        Task<List<PendingContractViewModel>> GetPendingContracts();
        Task<List<Vehicle>> GetVehicles();
        Task<List<Driver>> GetDrivers();
        Task<List<VehicleService>> GetHistory();
        Task<List<VehicleType>> GetVehicleTypes();

        // Operaciones de Despacho (Core Business Logic)
        Task<(bool success, string message)> StartTrip(int contractId, int contractMovilityDetailId, int vehicleId, int driverId, string observations);
        Task<(bool success, string message)> EndTrip(int vehicleId, string observations);

        // CRUD de Configuración
        Task<(bool success, string message)> CreateVehicleType(VehicleType model);
        Task<(bool success, string message)> EditVehicleType(VehicleType model);
        Task<(bool success, string message)> ToggleVehicleTypeStatus(int id);
        Task<(bool success, string message)> DeleteVehicleType(int id);

        Task<(bool success, string message)> CreateVehicle(Vehicle vehicle);
        Task<(bool success, string message)> CreateDriver(Driver driver);
    }
}