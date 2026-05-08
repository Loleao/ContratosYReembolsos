using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Services.DTOs.Exhumations;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations
{
    public class ExhumationService : IExhumationService
    {
        private readonly ApplicationDbContext _context;

        public ExhumationService(ApplicationDbContext context) => _context = context;

        public async Task<List<ExhumationSearchDto>> SearchContractsAsync(string dni, string name)
        {
            var query = _context.Contratos
                .Include(c => c.Branch)
                .Include(c => c.IntermentStructure)
                .Include(c => c.IntermentSpace)
                .Where(c => c.Status == "ACTIVO" && c.IntermentSpaceId.HasValue);

            if (!string.IsNullOrEmpty(dni)) query = query.Where(c => c.DeceasedDni.Contains(dni));
            if (!string.IsNullOrEmpty(name)) query = query.Where(c => c.DeceasedName.Contains(name));

            return await query.Select(c => new ExhumationSearchDto
            {
                ContractId = c.Id,
                ContractNumber = c.ContractNumber,
                DeceasedName = c.DeceasedName,
                DeceasedDni = c.DeceasedDni,
                BurialDate = c.BurialDate.ToShortDateString(),
                BranchName = c.Branch.Name,
                StructureName = c.IntermentStructure.Name,
                SpaceDetail = $"Fila {c.IntermentSpace.RowLetter} - N° {c.IntermentSpace.ColumnNumber}",

                // ASIGNACIÓN AQUÍ:
                CurrentLocation = $"{c.IntermentStructure.Name} (Fila {c.IntermentSpace.RowLetter} - N° {c.IntermentSpace.ColumnNumber})",

                CurrentSpaceId = c.IntermentSpaceId
            }).Take(10).ToListAsync();
        }

        public async Task<List<ExhumationSearchDto>> SearchDeceasedAsync(string query)
        {
            return await _context.Contratos
                .Where(c => c.Status == "ACTIVO" && (c.DeceasedName.Contains(query) || c.DeceasedDni.Contains(query) || c.ContractNumber.Contains(query)))
                .Include(c => c.IntermentStructure)
                .Include(c => c.IntermentSpace)
                .Select(c => new ExhumationSearchDto
                {
                    ContractId = c.Id,
                    ContractNumber = c.ContractNumber,
                    DeceasedName = c.DeceasedName,
                    DeceasedDni = c.DeceasedDni,
                    CurrentLocation = c.IntermentSpace != null
                        ? $"{c.IntermentStructure.Name} - Fila {c.IntermentSpace.RowLetter} Col {c.IntermentSpace.ColumnNumber}"
                        : "Ubicación no especificada"
                })
                .Take(10).ToListAsync();
        }

        public async Task<List<ExhumationSearchDto>> SearchDeceasedForExhumationAsync(string query)
        {
            return await _context.Contratos
                .Include(c => c.IntermentSpace)
                .Include(c => c.IntermentStructure)
                .Where(c => c.Status == "ACTIVO" &&
                           (c.DeceasedName.Contains(query) || c.DeceasedDni.Contains(query) || c.ContractNumber.Contains(query)))
                .Select(c => new ExhumationSearchDto
                {
                    ContractId = c.Id,
                    ContractNumber = c.ContractNumber,
                    DeceasedName = c.DeceasedName,
                    DeceasedDni = c.DeceasedDni,
                    // Construimos la ubicación legible para el usuario
                    CurrentLocation = c.IntermentSpace != null
                        ? $"{c.IntermentStructure.Name} - Fila {c.IntermentSpace.RowLetter} Col {c.IntermentSpace.ColumnNumber}"
                        : "Ubicación no registrada"
                })
                .Take(10)
                .ToListAsync();
        }

        public async Task<ExhumationSearchDto> GetOriginDetailsAsync(int contractId)
        {
            // Similar al anterior pero para un ID específico
            return await _context.Contratos
                .Include(c => c.IntermentSpace)
                .Include(c => c.IntermentStructure)
                .Where(c => c.Id == contractId)
                .Select(c => new ExhumationSearchDto
                {
                    ContractId = c.Id,
                    DeceasedName = c.DeceasedName,
                    CurrentLocation = $"{c.IntermentStructure.Name} - {c.IntermentSpace.RowLetter}{c.IntermentSpace.ColumnNumber}"
                }).FirstOrDefaultAsync();
        }

        public async Task<(bool success, string message)> RegisterExhumationAsync(ExhumationCreateDto model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.Contratos
                    .Include(c => c.Cemetery)
                    .Include(c => c.IntermentStructure)
                    .Include(c => c.IntermentSpace)
                    .FirstOrDefaultAsync(c => c.Id == model.ContractId);

                if (contract == null) return (false, "Contrato no encontrado.");

                // 1. CAPTURAR ORIGEN (Snapshot para el historial)
                string originSnapshot = contract.IntermentSpace != null
                    ? $"{contract.Cemetery?.Name ?? "Sede"} - {contract.IntermentStructure?.Name ?? "Pabellón"} - Fila {contract.IntermentSpace.RowLetter} Col {contract.IntermentSpace.ColumnNumber}"
                    : "Ubicación No Registrada";

                // 2. LIBERAR ESPACIO ACTUAL
                if (contract.IntermentSpaceId.HasValue)
                {
                    var oldSpace = await _context.SepulturasNichos.FindAsync(contract.IntermentSpaceId.Value);
                    if (oldSpace != null)
                    {
                        oldSpace.Status = IntermentStatus.Disponible;
                        _context.Update(oldSpace);
                    }
                }

                // 3. REGISTRAR EL EVENTO EN EL HISTORIAL (La Exhumación)
                var count = await _context.Exhumaciones.CountAsync() + 1;
                var exhumationRecord = new Exhumation
                {
                    ExhumationNumber = $"EX-{DateTime.Now.Year}-{count:D4}",
                    RequestDate = DateTime.Now,
                    OriginalContractId = contract.Id,

                    // Guardamos los IDs de origen (Snapshot técnico)
                    PreviousCemeteryId = contract.CemeteryId,
                    PreviousStructureId = contract.IntermentStructureId ?? 0,
                    PreviousSpaceId = contract.IntermentSpaceId ?? 0,
                    PreviousLocationSnapshot = originSnapshot,

                    // Datos del Destino
                    IsInternalRelocation = model.IsInternal,
                    DestinationDetails = model.IsInternal
                        ? $"Interno: {model.CemeteryName}"
                        : model.CemeteryName,

                    NewCemeteryId = model.CemeteryId,
                    NewStructureId = model.NewStructureId,
                    NewSpaceId = model.NewIntermentSpaceId,
                    NewLocationSnapshot = model.IsInternal ? model.NewLocationName : "EXTERNO",

                    Cost = model.TotalCost,
                    Status = "Completado",
                    MovementType = model.DestinationType?.ToUpper() ?? "TRASLADO"
                };

                // 4. ACTUALIZAR EL CONTRATO (MANTENERLO ACTIVO)
                if (model.IsInternal && model.NewIntermentSpaceId.HasValue)
                {
                    var newSpace = await _context.SepulturasNichos.FindAsync(model.NewIntermentSpaceId.Value);
                    newSpace.Status = IntermentStatus.Ocupado;
                    _context.Update(newSpace);

                    contract.CemeteryId = model.CemeteryId ?? contract.CemeteryId;
                    contract.IntermentStructureId = model.NewStructureId;
                    contract.IntermentSpaceId = model.NewIntermentSpaceId;

                    // IMPORTANTE: Mantenemos el estado en ACTIVO para permitir futuras exhumaciones
                    contract.Status = "ACTIVO";
                }
                else
                {
                    // Solo si sale del cementerio lo marcamos como cerrado
                    contract.IntermentSpaceId = null;
                    contract.Status = "EXHUMADO_EXTERNO";
                }

                _context.Exhumaciones.Add(exhumationRecord);
                _context.Update(contract);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Movimiento registrado: {exhumationRecord.ExhumationNumber}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error: " + ex.Message);
            }
        }

        public async Task<ExhumationPDFDto> GetExhumationForPdfAsync(int exhumationId)
        {
            var data = await _context.Exhumaciones
                .Include(e => e.OriginalContract)
                .Where(e => e.Id == exhumationId)
                .Select(e => new ExhumationPDFDto
                {
                    ExhumationNumber = e.ExhumationNumber,
                    RequestDate = e.RequestDate,
                    DeceasedName = e.OriginalContract.DeceasedName, // Asumiendo que esta en Contract
                    DeceasedDni = e.OriginalContract.DeceasedDni,
                    OriginLocation = e.PreviousLocationSnapshot,
                    DestinationLocation = e.NewLocationSnapshot,
                    MovementType = e.MovementType,
                    TotalCost = e.Cost
                })
                .FirstOrDefaultAsync();

            return data;
        }
    }
}