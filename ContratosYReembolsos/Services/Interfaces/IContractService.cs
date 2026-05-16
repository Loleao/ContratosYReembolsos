using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Models.ViewModels.Contracts;
using ContratosYReembolsos.Models.ViewModels.Exhumations;
using ContratosYReembolsos.Services.DTOs.Contracts;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IContractService
    {

        Task<IEnumerable<IGrouping<string, Branch>>> GetBranchesGroupedByRegionAsync();
        Task<List<ContractListDto>> GetContractListByBranchAsync(int? branchId);
        // Consultas de Afiliados (Lima Database)
        Task<List<object>> SearchAffiliates(string dni, string cip, string name);
        Task<List<object>> GetBeneficiariesByAffiliate(string afiliadoId);
        Task<List<object>> GetWakes(int branchId);


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
        
        Task<List<object>> GetStockItemsByBranch(int branchId);
        Task<List<object>> GetAvailableVehicleTypesByBranch(int branchId);
        Task<List<object>> GetFuneralServices(int? agencyId);

        Task<(bool success, string message, int contractId, string contractNumber)> CreateContract(ContractViewModel model);
        Task<List<ContractListDto>> GetContractListAsync();

        Task<ContractReportDto> GetContractForPDFAsync(int id);
        Task<ContractDetailDto> GetContractDetailsAsync(int id);
        Task<(bool success, string message)> ConfirmProductDeliveryAsync(ConfirmProductDeliveryInput input);
        Task<(bool success, string message)> AdvanceServiceStatusAsync(AdvanceServiceStatusInput input);
        Task<List<ExhumationHistoryItemViewModel>> GetMovementHistoryAsync(int contractId);


        Task<List<FuneralService>> GetAllServicesAsync();
        Task<(bool success, string message)> UpsertServiceAsync(FuneralService model);



    }
}
