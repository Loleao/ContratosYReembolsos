using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Services.DTOs.Cemeteries;
using ContratosYReembolsos.Services.DTOs.Contracts;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Cemeteries
{
    public class InhumationService : IInhumationService
    {
        private readonly ApplicationDbContext _context;

        public InhumationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> RegisterDirectInhumationAsync(InhumationWithoutContractInput input)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Generar número de folio autoincremental de regularización
                int count = await _context.SepulturasHistorial.CountAsync(h => h.ContractNumber.StartsWith("REG-")) + 1;
                string regCode = $"REG-{DateTime.Now.Year}-{count:D4}";

                // 2. Registrar difunto en la tabla unificada
                var deceased = new Deceased
                {
                    FullName = input.DeceasedName.ToUpper().Trim(),
                    Dni = input.DeceasedDni.Trim(),
                    DeathDate = input.DeathDate,
                    BurialDate = input.BurialDate,
                    BurialTime = DateTime.Now.TimeOfDay,
                    EntryType = "REGULARIZACION_DIRECTA",
                    TrackingCode = regCode,
                    CreatedAt = DateTime.Now
                };
                _context.Fallecidos.Add(deceased);
                await _context.SaveChangesAsync(); // Genera el Deceased.Id

                // 3. Modificar estado físico del nicho/tumba
                if (input.IntermentSpaceId.HasValue && input.IntermentSpaceId.Value > 0)
                {
                    var space = await _context.SepulturasNichos.FindAsync(input.IntermentSpaceId.Value);
                    if (space == null) return (false, "El espacio físico no existe en los registros.");
                    if (space.Status != IntermentStatus.Disponible) return (false, "El espacio ya no está disponible.");

                    space.Status = IntermentStatus.Ocupado;
                    _context.SepulturasNichos.Update(space);

                    // 4. Asentar ciclo histórico enlazando el Id del nuevo difunto
                    _context.SepulturasHistorial.Add(new SpaceIntermentHistory
                    {
                        IntermentSpaceId = input.IntermentSpaceId.Value,
                        ContractNumber = regCode,
                        DeceasedId = deceased.Id, // Llave foránera relacional unificada
                        StartDate = input.BurialDate,
                        EndDate = null,
                        OperationType = "INHUMACIÓN DIRECTA (SIN CONTRATO)",
                        Observations = input.Observations ?? "Regularización de ingreso externo por traslado."
                    });
                }
                else
                {
                    return (false, "Error: Es obligatorio asignar un espacio físico de sepultura.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Inhumación directa registrada con éxito bajo el Folio: {regCode}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var msg = ex.InnerException?.Message ?? ex.Message;
                return (false, "Error transaccional: " + msg);
            }
        }

        public async Task<(string cemeteryName, string spaceCode)> GetInhumationDisplayNamesAsync(int cemeteryId, int? spaceId)
        {
            var cemeteryName = await _context.Cementerios
                .Where(c => c.Id == cemeteryId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "Camposanto";

            string spaceCode = "N/A";
            if (spaceId.HasValue && spaceId.Value > 0)
            {
                spaceCode = await _context.SepulturasNichos
                    .Where(s => s.Id == spaceId.Value)
                    .Select(s => s.Code)
                    .FirstOrDefaultAsync() ?? "N/A";
            }

            return (cemeteryName, spaceCode);
        }
    }
}