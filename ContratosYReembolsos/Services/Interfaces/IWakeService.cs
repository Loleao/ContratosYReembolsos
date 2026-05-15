namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IWakeService
    {
        Task<(bool success, string message)> ImportFromExcelAsync(string filePath);
        Task SeedIfEmptyAsync();
    }
}