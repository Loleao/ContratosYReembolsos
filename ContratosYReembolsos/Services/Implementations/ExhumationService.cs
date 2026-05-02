using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Services.DTOs.Exhumations;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
// Asumo que tus constantes están en este namespace o uno similar
using ContratosYReembolsos.Constants;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;

namespace ContratosYReembolsos.Services.Implementations
{
    public class ExhumationService : IExhumationService
    {
        private readonly ApplicationDbContext _context;

        public ExhumationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message, int exhumationId)> CreateExhumationAsync(ExhumationCreateDto model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar Contrato Original
                var originalContract = await _context.Contratos
                    .FirstOrDefaultAsync(c => c.Id == model.OriginalContractId);

                if (originalContract == null)
                    return (false, "El contrato original no existe.", 0);

                // 2. LIBERAR ESPACIO ANTIGUO
                if (originalContract.IntermentSpaceId.HasValue)
                {
                    var oldSpace = await _context.SepulturasNichos.FindAsync(originalContract.IntermentSpaceId);
                    if (oldSpace != null)
                    {
                        // Usamos tu constante
                        oldSpace.Status = IntermentStatus.Disponible;
                        _context.Entry(oldSpace).State = EntityState.Modified;
                    }
                }

                // 3. CREAR EL REGISTRO DE EXHUMACIÓN
                var exhumation = new Exhumation
                {
                    ExhumationNumber = await GenerateNextExhumationNumber(),
                    RequestDate = DateTime.Now,
                    OriginalContractId = model.OriginalContractId,
                    IsInternalRelocation = model.IsInternalRelocation,

                    NewCemeteryId = model.IsInternalRelocation ? model.NewCemeteryId : null,
                    NewIntermentSpaceId = model.IsInternalRelocation ? model.NewIntermentSpaceId : null,

                    DestinationDetails = !model.IsInternalRelocation ? model.ExternalDestination : "TRASLADO INTERNO FONAFUN",

                    Cost = model.IsInternalRelocation ? model.RelocationCost : 0,
                    // Estado del trámite administrativo
                    Status = model.IsInternalRelocation ? "PENDIENTE_TRASLADO" : "COMPLETADO_EXTERNO",
                    Observations = model.Observations
                };

                _context.Exhumaciones.Add(exhumation);

                // 4. OCUPAR EL NUEVO ESPACIO (Si es interno)
                if (model.IsInternalRelocation && model.NewIntermentSpaceId.HasValue)
                {
                    var newSpace = await _context.SepulturasNichos.FindAsync(model.NewIntermentSpaceId);
                    if (newSpace != null)
                    {
                        // Validamos que el espacio esté realmente disponible antes de ocuparlo
                        if (newSpace.Status != IntermentStatus.Disponible)
                        {
                            return (false, "El nuevo espacio seleccionado no está disponible.", 0);
                        }

                        newSpace.Status = IntermentStatus.Ocupado; // Usamos tu constante
                        _context.Entry(newSpace).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Exhumación y traslado registrados con éxito.", exhumation.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error en la operación: {ex.Message}", 0);
            }
        }

        private async Task<string> GenerateNextExhumationNumber()
        {
            var year = DateTime.Now.Year;
            var count = await _context.Exhumaciones.CountAsync(x => x.RequestDate.Year == year);
            return $"EX-{year}-{(count + 1):D4}";
        }
    }
}