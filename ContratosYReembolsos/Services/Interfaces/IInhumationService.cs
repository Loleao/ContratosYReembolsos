using ContratosYReembolsos.Services.DTOs.Contracts;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IInhumationService
    {
        Task<(bool success, string message)> RegisterDirectInhumationAsync(InhumationWithoutContractInput input);
        Task<(string cemeteryName, string spaceCode)> GetInhumationDisplayNamesAsync(int cemeteryId, int? spaceId);
    
    }
}