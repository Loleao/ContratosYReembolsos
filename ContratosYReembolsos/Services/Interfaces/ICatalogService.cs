namespace ContratosYReembolsos.Services.Interfaces
{
    public interface ICatalogService
    {
        Task SeedCatalogIfEmptyAsync();
        Task<(bool success, string message)> ImportCatalogFromExcelAsync(string filePath);
    }
}