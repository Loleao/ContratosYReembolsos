namespace ContratosYReembolsos.Services
{
    public interface IUbigeoService
    {
        Task<(bool success, string message)> ImportFromExcelAsync(string filePath);

        Task SeedIfEmptyAsync();
    }
}