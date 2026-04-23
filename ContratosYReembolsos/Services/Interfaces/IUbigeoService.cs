namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IUbigeoService
    {
        Task<(bool success, string message)> ImportFromExcelAsync(string filePath);

        Task SeedIfEmptyAsync();
    }
}