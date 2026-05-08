using ContratosYReembolsos.Models.Entities.Contracts;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IFuneralService
    {
        // ... otros métodos que ya tengas ...
        Task<List<FuneralService>> GetAllServicesAsync();
        Task<(bool success, string message)> UpsertServiceAsync(FuneralService model);

        // Métodos para el Seeding
        Task<(bool success, string message)> ImportFromExcelAsync(string filePath);
        Task SeedIfEmptyAsync();
    }
}