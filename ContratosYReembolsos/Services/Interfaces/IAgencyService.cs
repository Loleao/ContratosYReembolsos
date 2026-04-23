using ContratosYReembolsos.Models.Entities.Agencies;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IAgencyService
    {
        // Consultas
        Task<List<Branch>> GetBranchesWithAgencies();
        Task<List<Agency>> GetAgenciesByBranch(int branchId);
        Task<Agency?> GetById(int id);
        Task<string?> GetBranchName(int branchId);

        // Operaciones CRUD y Estado
        Task<(bool success, string message)> Create(Agency model);
        Task<(bool success, string message)> Edit(Agency model);
        Task<(bool success, string message)> ToggleStatus(int id, bool active);
        Task<(bool success, string message)> Delete(int id);
    }
}