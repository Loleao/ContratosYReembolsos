using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;

        public AssetService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- CATÁLOGO ---
        public async Task<List<AssetCatalog>> GetCatalog()
        {
            return await _context.ActivosCatalogo
                .Include(c => c.Subcategory).ThenInclude(s => s.Category)
                .OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<bool> SaveCatalogItem(AssetCatalog model)
        {
            if (model.Id == 0) _context.ActivosCatalogo.Add(model);
            else _context.Entry(model).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        // --- OPERACIONES ---
        public async Task<List<FixedAsset>> GetAssetsByBranch(int branchId)
        {
            return await _context.ActivosFijos
                .Include(a => a.AssetCatalog)
                .Where(a => a.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> RegisterAssetEntry(FixedAsset model)
        {
            try
            {
                _context.ActivosFijos.Add(model);
                await _context.SaveChangesAsync();

                // Registro inicial en historial
                _context.ActivosHistorial.Add(new AssetHistory
                {
                    FixedAssetId = model.Id,
                    Action = "Alta",
                    Note = "Ingreso inicial de activo al sistema",
                    Date = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return (true, "Activo registrado correctamente");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // --- MOVIMIENTOS ---
        public async Task<bool> AssignAsset(int assetId, string responsibleName, string notes)
        {
            var asset = await _context.ActivosFijos.FindAsync(assetId);
            if (asset == null) return false;

            asset.Status = AssetStatus.InUse;
            _context.ActivosHistorial.Add(new AssetHistory
            {
                FixedAssetId = assetId,
                Action = "Asignación",
                ResponsibleUserId = responsibleName,
                Note = notes
            });
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangeAssetStatus(int assetId, AssetStatus newStatus, string notes)
        {
            var asset = await _context.ActivosFijos.FindAsync(assetId);
            if (asset == null) return false;

            asset.Status = newStatus;
            _context.ActivosHistorial.Add(new AssetHistory
            {
                FixedAssetId = assetId,
                Action = "Cambio de Estado",
                Note = $"Nuevo estado: {newStatus}. Motivo: {notes}"
            });
            return await _context.SaveChangesAsync() > 0;
        }

        // --- HELPERS ---
        public async Task<string> GeneratePatrimonialCode(int subcategoryId)
        {
            var sub = await _context.ActivosSubcategorias
                .Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == subcategoryId);

            string prefix = $"FON-{(sub?.Category.Name.Substring(0, 3).ToUpper() ?? "GEN")}-{(sub?.Name.Substring(0, 3).ToUpper() ?? "SUB")}-{DateTime.Now.Year}-";

            var last = await _context.ActivosFijos.Where(a => a.PatrimonialCode.StartsWith(prefix))
                .OrderByDescending(a => a.PatrimonialCode).FirstOrDefaultAsync();

            int next = 1;
            if (last != null && int.TryParse(last.PatrimonialCode.Split('-').Last(), out int lastVal))
                next = lastVal + 1;

            return $"{prefix}{next:D4}";
        }

        public async Task<List<AssetCategory>> GetCategories()
        {
            return await _context.ActivosCategorias.Include(c => c.Subcategories).ToListAsync();
        }

        public async Task<List<AssetSubcategory>> GetSubcategories()
        {
            return await _context.ActivosSubcategorias.Include(s => s.Category).ToListAsync();
        }

        public async Task<List<AssetSubcategory>> GetSubcategoriesByCategoryId(int categoryId)
        {
            return await _context.ActivosSubcategorias.Where(s => s.CategoryId == categoryId).ToListAsync();
        }
    }
}