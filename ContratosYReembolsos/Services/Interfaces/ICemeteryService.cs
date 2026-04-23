using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface ICemeteryService
    {
        // Consultas y Navegación
        Task<List<Branch>> GetBranchesWithCemeteries();
        Task<List<Cemetery>> GetCemeteriesByBranch(int branchId);
        Task<Cemetery?> GetById(int id);
        Task<IntermentStructure?> GetStructureDetails(int id);
        Task<List<IntermentStructureTemplate>> GetTemplates();
        Task<List<IntermentStructureTemplate>> GetTemplatesByType(string type);
        Task<List<IntermentStructure>> GetStructuresByCemeteryAndType(int cemeteryId, string type);

        // Gestión de Cementerios y Modelos
        Task<(bool success, string message)> CreateCemetery(Cemetery model);
        Task<(bool success, string message)> SaveTemplate(IntermentStructureTemplate model);

        // Construcción de Infraestructura
        Task<(bool success, string message)> BuildStructure(IntermentStructure model, int? templateId);
        Task<(bool success, string message)> AddMassiveSpaces(int structureId, string rowLetter, int quantity);
        Task<(bool success, string message)> AddManualSpace(int structureId, string row, int col);
        Task<(bool success, string message)> DeleteSpace(int id);

        // Procesos Operativos (Exhumación/Traslado)
        Task<(bool success, string message)> ProcessTransfer(int originSpaceId, int destinationSpaceId, string reason);
    }
}