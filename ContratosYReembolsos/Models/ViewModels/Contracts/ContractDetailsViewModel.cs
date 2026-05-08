using ContratosYReembolsos.Models.ViewModels.Exhumations;
using ContratosYReembolsos.Services.DTOs.Contracts;

namespace ContratosYReembolsos.Models.ViewModels.Contracts
{
    public class ContractDetailsViewModel
    {
        // Reutilizamos el DTO para los datos básicos para no mapear campo por campo otra vez
        public ContractDetailDto Data { get; set; }

        // Historial de Exhumaciones (El nuevo requerimiento)
        public List<ExhumationHistoryItemViewModel> MovementHistory { get; set; } = new();

        // Propiedades calculadas para la Vista
        public string StatusColor => Data.Status == "ACTIVO" ? "success" :
                                     Data.Status == "REUBICADO" ? "info" : "secondary";
    }
}