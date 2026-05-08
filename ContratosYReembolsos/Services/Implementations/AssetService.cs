using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Models.ViewModels.Assets;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        public AssetService(ApplicationDbContext context) => _context = context;

        public async Task<List<AssetCategory>> GetCategoriesWithSub() =>
            await _context.ActivosCategorias.Include(c => c.Subcategories).ToListAsync();

        public async Task<List<AssetSubcategory>> GetSubcategories() =>
            await _context.ActivosSubcategorias.Include(s => s.Category).ToListAsync();

        public async Task<AssetCategory> GetCategoryById(int id) =>
            await _context.ActivosCategorias
                .Include(c => c.Subcategories) 
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> SaveCategory(AssetCategory model)
        {
            if (model.Id == 0) _context.ActivosCategorias.Add(model);
            else _context.ActivosCategorias.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AssetSubcategory> GetSubCategoryById(int id) =>
            await _context.ActivosSubcategorias.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == id);

        public async Task<bool> SaveSubCategory(AssetSubcategory model)
        {
            if (model.Id == 0) _context.ActivosSubcategorias.Add(model);
            else _context.ActivosSubcategorias.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<AssetCatalog>> GetCatalog() =>
            await _context.ActivosCatalogo.Include(c => c.Subcategory).ThenInclude(s => s.Category).ToListAsync();

        public async Task<AssetCatalog> GetCatalogItemById(int id) =>
            await _context.ActivosCatalogo.FindAsync(id);

        public async Task<bool> SaveCatalogItem(AssetCatalog model)
        {
            Console.WriteLine($"Guardando Catalog Item: Id={model.Id}, Name={model.Name}, SubcategoryId={model.SubcategoryId}");

            if (model.Id == 0) _context.ActivosCatalogo.Add(model);
            else _context.ActivosCatalogo.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string> GeneratePatrimonialCode(int subcategoryId)
        {
            var sub = await _context.ActivosSubcategorias.Include(s => s.Category).FirstOrDefaultAsync(s => s.Id == subcategoryId);
            string year = DateTime.Now.Year.ToString();
            string prefix = $"FON-{(sub?.Category.Name.Substring(0, 3).ToUpper() ?? "GEN")}-{year}-";

            var last = await _context.ActivosFijos.Where(a => a.PatrimonialCode.StartsWith(prefix))
                .OrderByDescending(a => a.PatrimonialCode).FirstOrDefaultAsync();

            int next = 1;
            if (last != null && int.TryParse(last.PatrimonialCode.Split('-').Last(), out int lastVal))
                next = lastVal + 1;

            return $"{prefix}{next:D4}";
        }

        public async Task<(bool success, string message)> ProcessBulkAssetEntry(List<FixedAsset> assets, int branchId, string observation, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string internalCode = await GenerateAssetInternalCode();
                string validObservation = string.IsNullOrWhiteSpace(observation) ? "Ingreso inicial de lote" : observation;

                // Diccionario para rastrear la secuencia por subcategoría durante este bucle
                var lastSequencePerSubcat = new Dictionary<int, int>();

                foreach (var asset in assets)
                {
                    // 1. Obtener datos del catálogo para el prefijo del código
                    var catalogItem = await _context.ActivosCatalogo
                        .Include(c => c.Subcategory)
                        .ThenInclude(s => s.Category)
                        .FirstOrDefaultAsync(c => c.Id == asset.AssetCatalogId);

                    int subcatId = catalogItem.SubcategoryId;

                    // 2. Control de correlativos en memoria para evitar duplicados
                    if (!lastSequencePerSubcat.ContainsKey(subcatId))
                    {
                        string prefix = $"FON-{catalogItem.Subcategory.Category.Name.Substring(0, 3).ToUpper()}-{DateTime.Now.Year}-";
                        var lastAsset = await _context.ActivosFijos
                            .Where(a => a.PatrimonialCode.StartsWith(prefix))
                            .OrderByDescending(a => a.PatrimonialCode)
                            .FirstOrDefaultAsync();

                        int currentSeq = 0;
                        if (lastAsset != null && int.TryParse(lastAsset.PatrimonialCode.Split('-').Last(), out int lastVal))
                        {
                            currentSeq = lastVal;
                        }
                        lastSequencePerSubcat[subcatId] = currentSeq;
                    }

                    // 3. Incrementar secuencia y generar código final
                    lastSequencePerSubcat[subcatId]++;
                    string catPrefix = catalogItem.Subcategory.Category.Name.Substring(0, 3).ToUpper();
                    asset.PatrimonialCode = $"FON-{catPrefix}-{DateTime.Now.Year}-{lastSequencePerSubcat[subcatId]:D4}";

                    // 4. Configuración del Activo
                    asset.BranchId = branchId;
                    asset.Observation = validObservation;
                    asset.CreatedAt = DateTime.Now;
                    asset.Status = AssetStatus.Available; // Estado inicial por defecto
                    asset.RegisteredByUserId = userId;

                    _context.ActivosFijos.Add(asset);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el Id para el movimiento

                    // 5. Registro del Movimiento de Entrada
                    _context.ActivosMovimientos.Add(new AssetMovement
                    {
                        FixedAssetId = asset.Id,
                        BranchId = branchId,
                        MovementType = AssetMovementType.Entry,
                        Concept = AssetConcept.Buy,
                        InternalControlNumber = internalCode,
                        UserId = userId,
                        Description = validObservation,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, $"{assets.Count} activos registrados con el control {internalCode}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error: " + ex.Message);
            }
        }

        // Helper para generar correlativo de ingresos (Similar a tu InventoryService)
        private async Task<string> GenerateAssetInternalCode()
        {
            var year = DateTime.Now.Year;
            var count = await _context.ActivosMovimientos
                .Where(m => m.CreatedAt.Year == year && m.Concept == AssetConcept.Buy)
                .Select(m => m.InternalControlNumber)
                .Distinct()
                .CountAsync();

            return $"NI-ACT-{year}-{(count + 1):D5}";
        }

        public async Task<string> GenerateTransferCode()
        {
            var year = DateTime.Now.Year;
            var count = await _context.ActivosTransferencias
                .CountAsync(t => t.SentAt.Year == year);
            return $"TR-ACT-{year}-{(count + 1):D5}";
        }

        public async Task<List<FixedAsset>> GetAvailableAssetsByBranch(int branchId)
        {
            return await _context.ActivosFijos
                .Include(a => a.AssetCatalog)
                .Where(a => a.BranchId == branchId && a.Status == AssetStatus.Available)
                .ToListAsync();
        }

        public async Task<(bool success, string message, string transferCode)> ProcessAssetTransfer(AssetTransferFormViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = new AssetTransfer
                {
                    InternalControlNumber = await GenerateTransferCode(),
                    OriginBranchId = model.OriginBranchId,
                    TargetBranchId = model.TargetBranchId,
                    Status = TransferStatus.Sent,
                    Observation = model.Observation,
                    SentByUserId = userId,
                    SentAt = DateTime.Now
                };

                _context.ActivosTransferencias.Add(transfer);
                await _context.SaveChangesAsync();

                // 'model.SelectedAssetIds' vendría de los IDs seleccionados en la tabla dinámica
                foreach (var assetId in model.SelectedAssetIds)
                {
                    var asset = await _context.ActivosFijos.FindAsync(assetId);
                    if (asset == null || asset.BranchId != model.OriginBranchId)
                        throw new Exception($"Activo {assetId} no válido para traslado.");

                    asset.Status = AssetStatus.Transferred;

                    transfer.Details.Add(new AssetTransferDetail { FixedAssetId = assetId });

                    _context.ActivosMovimientos.Add(new AssetMovement
                    {
                        FixedAssetId = assetId,
                        BranchId = model.OriginBranchId,
                        MovementType = AssetMovementType.Exit,
                        Concept = AssetConcept.Transfer,
                        InternalControlNumber = transfer.InternalControlNumber,
                        UserId = userId,
                        Description = $"Salida por TR: {transfer.InternalControlNumber}"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Traslado iniciado", transfer.InternalControlNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message, null);
            }
        }

        public async Task<List<FixedAsset>> GetAssetsByBranch(int branchId) =>
            await _context.ActivosFijos
                .Include(a => a.AssetCatalog).ThenInclude(c => c.Subcategory)
                .Where(a => a.BranchId == branchId && a.Status == AssetStatus.Available)
                .ToListAsync();

        public async Task<List<AssetTransfer>> GetPendingTransfersByBranch(int branchId) =>
            await _context.ActivosTransferencias
                .Include(t => t.OriginBranch)
                .Include(t => t.Details).ThenInclude(d => d.FixedAsset).ThenInclude(a => a.AssetCatalog)
                .Where(t => t.TargetBranchId == branchId && t.Status == TransferStatus.Sent)
                .ToListAsync();

        public async Task<List<AssetMovement>> GetKardexByBranch(int branchId) =>
            await _context.ActivosMovimientos
                .Include(m => m.FixedAsset)
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

        public async Task<(bool success, string message)> ConfirmAssetReceipt(int transferId, string observation, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _context.ActivosTransferencias
                    .Include(t => t.Details).ThenInclude(d => d.FixedAsset)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null) return (false, "Traslado no encontrado.");

                transfer.Status = TransferStatus.Received;
                transfer.ReceivedAt = DateTime.Now;
                transfer.ReceivedByUserId = userId;
                transfer.ReceptionObservation = observation;

                foreach (var detail in transfer.Details)
                {
                    var asset = detail.FixedAsset;
                    asset.BranchId = transfer.TargetBranchId; // Cambio oficial de sede
                    asset.Status = AssetStatus.Available;

                    // Movimiento de Entrada en la nueva sede
                    _context.ActivosMovimientos.Add(new AssetMovement
                    {
                        FixedAssetId = asset.Id,
                        BranchId = transfer.TargetBranchId,
                        MovementType = AssetMovementType.Entry,
                        Concept = AssetConcept.Transfer,
                        InternalControlNumber = transfer.InternalControlNumber,
                        UserId = userId,
                        Description = $"Recepción de traslado. Obs: {observation}"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Activos recibidos correctamente.");
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message); }
        }
    }
}