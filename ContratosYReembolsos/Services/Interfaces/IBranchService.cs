using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.ValueObjects;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IBranchService
    {
        // Consultas de datos
        Task<IEnumerable<IGrouping<string, Branch>>> GetGroupedBranchesAsync();
        Task<Branch?> GetByIdAsync(int id);
        Task<List<string>> GetRegionsAsync();
        Task<List<string>> GetProvincesAsync(string region);
        Task<List<object>> GetDistrictsAsync(string region, string province);

        // Operaciones de negocio
        Task<(bool success, string message)> CreateBranchAsync(Branch model);
        Task<(bool success, string message)> UpdateBranchAsync(Branch model);
    }
}