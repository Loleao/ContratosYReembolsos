using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Branches
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<IGrouping<string, Branch>>> GetGroupedBranchesAsync()
        {
            var branches = await _context.Filiales
                .Include(b => b.Ubigeo)
                .Include(b => b.Cemeteries)
                .ToListAsync();

            return branches.OrderBy(b => b.Ubigeo.Region).GroupBy(b => b.Ubigeo.Region);
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Filiales
                .Include(b => b.Ubigeo)
                .Include(b => b.Cemeteries)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<string>> GetRegionsAsync()
        {
            return await _context.Ubigeos
                .Select(u => u.Region)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<List<string>> GetProvincesAsync(string region)
        {
            return await _context.Ubigeos
                .Where(u => u.Region == region)
                .Select(u => u.Province)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<List<object>> GetDistrictsAsync(string region, string province)
        {
            return await _context.Ubigeos
                .Where(u => u.Region == region && u.Province == province)
                .Select(u => new { u.Id, District = u.District })
                .OrderBy(d => d.District)
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<(bool success, string message)> CreateBranchAsync(Branch model)
        {
            try
            {
                var ubigeo = await _context.Ubigeos.FindAsync(model.UbigeoId);
                if (ubigeo == null) return (false, "Ubigeo no encontrado.");

                // Lógica de generación de código (Regla de negocio)
                var prefix = await _context.Ubigeos
                    .Where(u => u.Region == ubigeo.Region && !string.IsNullOrEmpty(u.Abbreviation))
                    .Select(u => u.Abbreviation)
                    .FirstOrDefaultAsync() ?? "GEN";

                var lastNumber = await _context.Filiales.CountAsync(b => b.Code.StartsWith(prefix));
                model.Code = $"{prefix}{lastNumber + 1}";

                _context.Filiales.Add(model);
                await _context.SaveChangesAsync();
                return (true, $"Filial {model.Code} creada con éxito.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> UpdateBranchAsync(Branch model)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return (true, "Filial actualizada correctamente.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}