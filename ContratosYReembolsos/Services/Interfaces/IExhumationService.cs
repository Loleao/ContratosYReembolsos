using ContratosYReembolsos.Services.DTOs.Exhumations;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IExhumationService
    {
        // Para el buscador inicial
        //Task<IEnumerable<ExhumationContractSearchDto>> SearchOriginalContractsAsync(string filter);

        // Para cargar la data del contrato seleccionado
        //Task<ExhumationContractSearchDto> GetContractForExhumationAsync(int contractId);

        // El proceso principal
        Task<(bool success, string message, int exhumationId)> CreateExhumationAsync(ExhumationCreateDto model);
    }
}