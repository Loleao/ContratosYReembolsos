using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Cemeteries
{
    public class CemeteryService : ICemeteryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IntermentService _intermentLogic; // Tu servicio de lógica de nichos existente

        public CemeteryService(ApplicationDbContext context, IntermentService intermentLogic)
        {
            _context = context;
            _intermentLogic = intermentLogic;
        }

        public async Task<List<Branch>> GetBranchesWithCemeteries() => await _context.Filiales.Include(b => b.Cemeteries).ToListAsync();
        public async Task<List<Cemetery>> GetCemeteriesByBranch(int branchId) => await _context.Cementerios.Include(c => c.Structures).Where(c => c.BranchId == branchId).ToListAsync();
        public async Task<Cemetery?> GetById(int id) => await _context.Cementerios.Include(c => c.Branch).FirstOrDefaultAsync(c => c.Id == id);
        public async Task<IntermentStructure?> GetStructureDetails(int id) => await _context.SepulturasEstructura.Include(s => s.Template).Include(s => s.Spaces).FirstOrDefaultAsync(s => s.Id == id);
        public async Task<List<IntermentStructureTemplate>> GetTemplatesByType(string type) => await _context.TemplatesSepulturas.Where(t => t.Type == type.ToUpper()).ToListAsync();
        public async Task<List<IntermentStructure>> GetStructuresByCemeteryAndType(int cemeteryId, string type) => await _context.SepulturasEstructura.Include(s => s.Spaces).Where(s => s.CemeteryId == cemeteryId && s.Type == type.ToUpper()).ToListAsync();

        public async Task<List<IntermentStructureTemplate>> GetTemplates()
        {
            return await _context.TemplatesSepulturas
                .OrderBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> CreateCemetery(Cemetery model)
        {
            try
            {
                // Buscamos la Filial para heredar su UbigeoId
                var branch = await _context.Filiales
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == model.BranchId);

                if (branch == null) return (false, "La filial asignada no existe.");

                // Asignamos el Ubigeo de la filial al nuevo cementerio
                model.UbigeoId = branch.UbigeoId;

                _context.Cementerios.Add(model);
                await _context.SaveChangesAsync();

                return (true, $"Sede {model.Name} registrada con éxito.");
            }
            catch (Exception ex)
            {
                return (false, "Error al guardar: " + ex.Message);
            }
        }

        public async Task<(bool success, string message)> SaveTemplate(IntermentStructureTemplate model)
        {
            try
            {
                _context.TemplatesSepulturas.Add(model);
                await _context.SaveChangesAsync();
                return (true, "Modelo guardado con éxito.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> BuildStructure(IntermentStructure model, int? templateId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SepulturasEstructura.Add(model);
                await _context.SaveChangesAsync();
                if (templateId.HasValue) await _intermentLogic.BuildFromTemplateAsync(templateId.Value, model.Id);
                await transaction.CommitAsync();
                return (true, "Estructura creada con éxito.");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> AddMassiveSpaces(int structureId, string rowLetter, int quantity)
        {
            try
            {
                var structure = await _context.SepulturasEstructura.Include(s => s.Spaces).FirstOrDefaultAsync(s => s.Id == structureId);
                if (structure == null) return (false, "No existe.");
                rowLetter = rowLetter.ToUpper().Trim();
                int lastCol = structure.Spaces.Where(s => s.RowLetter == rowLetter).Select(s => s.ColumnNumber).DefaultIfEmpty(0).Max();

                for (int i = 1; i <= quantity; i++)
                {
                    int num = lastCol + i;
                    _context.SepulturasNichos.Add(new IntermentSpace
                    {
                        StructureId = structureId,
                        RowLetter = rowLetter,
                        ColumnNumber = num,
                        Code = $"{structure.Name.Substring(0, 2).ToUpper()}-{rowLetter}-{num:D2}",
                        Status = IntermentStatus.Disponible,
                        FloorNumber = 1
                    });
                }
                await _context.SaveChangesAsync();
                return (true, $"Generados {quantity} espacios.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> AddManualSpace(int structureId, string row, int col)
        {
            var structure = await _context.SepulturasEstructura.FindAsync(structureId);
            _context.SepulturasNichos.Add(new IntermentSpace
            {
                StructureId = structureId,
                RowLetter = row,
                ColumnNumber = col,
                Code = $"{structure.Name.Substring(0, 3).ToUpper()}-T-{row}{col}",
                Status = IntermentStatus.Disponible,
                Price = 2000
            });
            await _context.SaveChangesAsync(); return (true, "OK");
        }

        public async Task<(bool success, string message)> DeleteSpace(int id)
        {
            var space = await _context.SepulturasNichos.FindAsync(id);
            if (space == null) return (false, "No existe");
            if (space.Status != IntermentStatus.Disponible) return (false, "Está ocupado/reservado");
            _context.SepulturasNichos.Remove(space); await _context.SaveChangesAsync(); return (true, "Eliminado");
        }

        public async Task<(bool success, string message)> ProcessTransfer(int originSpaceId, int destinationSpaceId, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var origin = await _context.SepulturasNichos.FindAsync(originSpaceId);
                var dest = await _context.SepulturasNichos.FindAsync(destinationSpaceId);
                if (dest.Status != IntermentStatus.Disponible) return (false, "Destino no disponible.");

                origin.Status = IntermentStatus.Disponible;
                dest.Status = IntermentStatus.Ocupado;
                await _context.SaveChangesAsync(); await transaction.CommitAsync();
                return (true, "Traslado completado.");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }
    }
}