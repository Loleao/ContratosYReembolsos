using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Models.ViewModels;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IContractService
    {
        // Consultas de Afiliados (Lima Database)
        Task<List<object>> SearchAffiliates(string dni, string cip, string name);
        Task<List<object>> GetBeneficiariesByAffiliate(string afiliadoId);
        Task<List<object>> GetWakes();

        // Consultas de Ubicación y Sedes (Local Database)
        Task<List<string>> GetRegions();
        Task<List<string>> GetProvinces(string region);
        Task<List<object>> GetDistricts(string region, string province);
        Task<object?> GetBranchCapabilities(int branchId);
        Task<string> GetBranchAbbreviation(int branchId);

        // Consultas de Catálogos para Contratos
        Task<List<object>> GetCemeteries(string? inei, int? branchId);
        Task<List<object>> GetStructures(int cemeteryId, string type);
        Task<List<object>> GetSpaceMap(int structureId);
        Task<List<object>> GetAgencies(string ruc, string name, int? branchId);
        Task<List<object>> GetCoffinsByBranch(int branchId);
        Task<List<object>> GetAvailableVehicleTypesByBranch(int branchId);

        // Operación Principal
        Task<(bool success, string message, int contractId, string contractNumber)> CreateContract(ContractViewModel model);
        Task<Contract?> GetContractForPdf(int id);
    }
}