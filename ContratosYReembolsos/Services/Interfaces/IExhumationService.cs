using ContratosYReembolsos.Services.DTOs.Exhumations;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IExhumationService
    {
        Task<List<ExhumationSearchDto>> SearchContractsAsync(string dni, string name);
        Task<List<ExhumationSearchDto>> SearchDeceasedAsync(string query);
        Task<ExhumationSearchDto> GetOriginDetailsAsync(int contractId);
        Task<(bool success, string message)> RegisterExhumationAsync(ExhumationCreateDto model);
        Task<ExhumationPDFDto> GetExhumationForPdfAsync(int exhumationId);
    }
}