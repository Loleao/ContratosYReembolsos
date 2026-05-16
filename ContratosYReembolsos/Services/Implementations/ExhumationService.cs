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
            // Buscamos folios de ocupación vigentes (EndDate == null) enlazando al fallecido
            var query = _context.SepulturasHistorial
                .Include(h => h.Deceased)
                .Include(h => h.IntermentSpace).ThenInclude(s => s.Structure).ThenInclude(st => st.Cemetery).ThenInclude(c => c.Branch)
                .Where(h => h.EndDate == null);

            if (!string.IsNullOrEmpty(dni)) query = query.Where(h => h.Deceased.Dni.Contains(dni));
            if (!string.IsNullOrEmpty(name)) query = query.Where(h => h.Deceased.FullName.Contains(name));

            return await query.Select(h => new ExhumationSearchDto
            {
                // Buscamos si existe contrato; si fue regularización directa enviará un ID virtual o 0
                ContractId = _context.Contratos.Where(c => c.ContractNumber == h.ContractNumber).Select(c => c.Id).FirstOrDefault(),
                ContractNumber = h.ContractNumber,
                DeceasedId = h.DeceasedId, // Agregado en tu DTO para control estricto de la nueva relación
                DeceasedName = h.Deceased.FullName,
                DeceasedDni = h.Deceased.Dni,
                BurialDate = h.StartDate.ToShortDateString(),
                BranchName = h.IntermentSpace.Structure.Cemetery.Branch.Name,
                StructureName = h.IntermentSpace.Structure.Name,
                SpaceDetail = $"Fila {h.IntermentSpace.RowLetter} - N° {h.IntermentSpace.ColumnNumber}",
                CurrentLocation = $"{h.IntermentSpace.Structure.Name} (Fila {h.IntermentSpace.RowLetter} - N° {h.IntermentSpace.ColumnNumber})",
                CurrentSpaceId = h.IntermentSpaceId
            }).Take(10).ToListAsync();
        }

        public async Task<List<ExhumationSearchDto>> SearchDeceasedAsync(string query)
        {
            return await _context.SepulturasHistorial
                .Include(h => h.Deceased)
                .Include(h => h.IntermentSpace).ThenInclude(s => s.Structure)
                .Where(h => h.EndDate == null && (h.Deceased.FullName.Contains(query) || h.Deceased.Dni.Contains(query) || h.ContractNumber.Contains(query)))
                .Select(h => new ExhumationSearchDto
                {
                    ContractId = _context.Contratos.Where(c => c.ContractNumber == h.ContractNumber).Select(c => c.Id).FirstOrDefault(),
                    ContractNumber = h.ContractNumber,
                    DeceasedId = h.DeceasedId,
                    DeceasedName = h.Deceased.FullName,
                    DeceasedDni = h.Deceased.Dni,
                    CurrentLocation = $"{h.IntermentSpace.Structure.Name} - Fila {h.IntermentSpace.RowLetter} Col {h.IntermentSpace.ColumnNumber}",
                    CurrentSpaceId = h.IntermentSpaceId
                }).Take(10).ToListAsync();
        }

        public async Task<List<ExhumationSearchDto>> SearchDeceasedForExhumationAsync(string query)
        {
            return await SearchDeceasedAsync(query); // Mapeamos a la misma lógica relacional corregida arriba
        }

        public async Task<ExhumationSearchDto> GetOriginDetailsAsync(int deceasedId)
        {
            return await _context.SepulturasHistorial
                .Include(h => h.Deceased)
                .Include(h => h.IntermentSpace).ThenInclude(s => s.Structure)
                .Where(h => h.DeceasedId == deceasedId && h.EndDate == null)
                .Select(h => new ExhumationSearchDto
                {
                    ContractId = _context.Contratos.Where(c => c.ContractNumber == h.ContractNumber).Select(c => c.Id).FirstOrDefault(),
                    ContractNumber = h.ContractNumber,
                    DeceasedId = h.DeceasedId,
                    DeceasedName = h.Deceased.FullName,
                    CurrentLocation = $"{h.IntermentSpace.Structure.Name} - {h.IntermentSpace.RowLetter}{h.IntermentSpace.ColumnNumber}",
                    CurrentSpaceId = h.IntermentSpaceId
                }).FirstOrDefaultAsync();
        }

        public async Task<(bool success, string message)> RegisterExhumationAsync(ExhumationCreateDto model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Buscamos la estancia activa del fallecido usando su DeceasedId unificado
                var activeHistory = await _context.SepulturasHistorial
                    .Include(h => h.IntermentSpace).ThenInclude(s => s.Structure).ThenInclude(st => st.Cemetery)
                    .FirstOrDefaultAsync(h => h.DeceasedId == model.DeceasedId && h.EndDate == null);

                if (activeHistory == null) return (false, "No se encontró un registro de sepultura activo para este fallecido.");

                string originSnapshot = $"{activeHistory.IntermentSpace.Structure.Cemetery.Name} - {activeHistory.IntermentSpace.Structure.Name} - Fila {activeHistory.IntermentSpace.RowLetter} Col {activeHistory.IntermentSpace.ColumnNumber}";
                int oldSpaceId = activeHistory.IntermentSpaceId;

                // =========================================================================
                // PASO 1: CERRAR Y LIBERAR EL NICHO DE ORIGEN
                // =========================================================================
                var oldSpace = await _context.SepulturasNichos.FindAsync(oldSpaceId);
                if (oldSpace != null)
                {
                    oldSpace.Status = IntermentStatus.Disponible;
                    _context.Update(oldSpace);
                }

                activeHistory.EndDate = DateTime.Now; // Estampamos el fin de la estancia física
                activeHistory.Observations += $" | Exhumado el {DateTime.Now:dd/MM/yyyy HH:mm} bajo Acta de Movimiento de Oficina.";
                _context.Update(activeHistory);

                // Verificamos si existe un contrato administrativo vinculado a este folio
                var contract = await _context.Contratos.FirstOrDefaultAsync(c => c.ContractNumber == activeHistory.ContractNumber);

                // =========================================================================
                // PASO 2: ASENTAR LA CABECERA OFICIAL DE EXHUMACIÓN
                // =========================================================================
                var count = await _context.Exhumaciones.CountAsync() + 1;
                var exhumationRecord = new Exhumation
                {
                    ExhumationNumber = $"EX-{DateTime.Now.Year}-{count:D4}",
                    RequestDate = DateTime.Now,
                    DeceasedId = activeHistory.DeceasedId, // Llave fuerte mapeada
                    OriginalContractId = contract?.Id, // Nullable si fue regularización directa

                    PreviousCemeteryId = activeHistory.IntermentSpace.Structure.CemeteryId,
                    PreviousStructureId = activeHistory.IntermentSpace.StructureId,
                    PreviousSpaceId = oldSpaceId,
                    PreviousLocationSnapshot = originSnapshot,

                    IsInternalRelocation = model.IsInternal,
                    DestinationDetails = model.IsInternal ? $"Interno: {model.CemeteryName}" : model.CemeteryName,

                    NewCemeteryId = model.CemeteryId,
                    NewStructureId = model.NewStructureId,
                    NewSpaceId = model.NewIntermentSpaceId,
                    NewLocationSnapshot = model.IsInternal ? model.NewLocationName : "EXTERNO",

                    Cost = model.TotalCost,
                    Status = "Completado",
                    MovementType = model.DestinationType?.ToUpper() ?? "TRASLADO"
                };
                _context.Exhumaciones.Add(exhumationRecord);

                // =========================================================================
                // PASO 3: TRASLADO INTERNO A OTRO NICHO DE FONAFUN (SI APLICA)
                // =========================================================================
                if (model.IsInternal && model.NewIntermentSpaceId.HasValue)
                {
                    var newSpace = await _context.SepulturasNichos.FindAsync(model.NewIntermentSpaceId.Value);
                    if (newSpace == null) return (false, "El espacio físico de destino no existe.");

                    newSpace.Status = IntermentStatus.Ocupado;
                    _context.Update(newSpace);

                    // Si tenía contrato, actualizamos la ubicación en su cabecera para los reportes generales
                    if (contract != null)
                    {
                        contract.CemeteryId = model.CemeteryId ?? contract.CemeteryId;
                        contract.IntermentStructureId = model.NewStructureId;
                        contract.IntermentSpaceId = model.NewIntermentSpaceId;
                        _context.Update(contract);
                    }

                    // ABRIMOS UN NUEVO FOLIO EN EL HISTORIAL DEL NUEVO NICHO
                    _context.SepulturasHistorial.Add(new SpaceIntermentHistory
                    {
                        IntermentSpaceId = model.NewIntermentSpaceId.Value,
                        ContractNumber = activeHistory.ContractNumber, // Mantenemos el código de traza original (Contrato o REG-XXXX)
                        DeceasedId = activeHistory.DeceasedId,
                        StartDate = DateTime.Now,
                        EndDate = null,
                        OperationType = $"REUBICACIÓN ({exhumationRecord.MovementType})",
                        Observations = $"Ingreso por exhumación y traslado interno desde {originSnapshot}. Acta: {exhumationRecord.ExhumationNumber}"
                    });
                }
                else
                {
                    // Caso: Exhumación Externa definitiva. Si tenía contrato, se actualizan sus punteros
                    if (contract != null)
                    {
                        contract.IntermentSpaceId = null;
                        contract.Status = "EXHUMADO_EXTERNO";
                        _context.Update(contract);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Movimiento de exhumación asentado correctamente: {exhumationRecord.ExhumationNumber}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error crítico en base de datos: " + ex.Message);
            }
        }

        public async Task<ExhumationPDFDto> GetExhumationForPdfAsync(int exhumationId)
        {
            return await _context.Exhumaciones
                .Include(e => e.Deceased)
                .Where(e => e.Id == exhumationId)
                .Select(e => new ExhumationPDFDto
                {
                    ExhumationNumber = e.ExhumationNumber,
                    RequestDate = e.RequestDate,
                    DeceasedName = e.Deceased.FullName, // Leemos directamente de la tabla Deceased
                    DeceasedDni = e.Deceased.Dni,
                    OriginLocation = e.PreviousLocationSnapshot,
                    DestinationLocation = e.NewLocationSnapshot,
                    MovementType = e.MovementType,
                    TotalCost = e.Cost
                })
                .FirstOrDefaultAsync();
        }
    }
}