using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.ViewModels.Contracts;
using ContratosYReembolsos.Services.DTOs.Contracts;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Contracts
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;

        public ContractService(ApplicationDbContext context, LimaContractsDbContext limaContext)
        {
            _context = context;
            _limaContext = limaContext;
        }

        public async Task<List<object>> SearchAffiliates(string dni, string cip, string name)
        {
            var query = _limaContext.Afiliados.AsQueryable();
            if (!string.IsNullOrWhiteSpace(dni)) query = query.Where(a => a.DNI.Contains(dni));
            if (!string.IsNullOrWhiteSpace(cip)) query = query.Where(a => a.CIP.Contains(cip));
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(a => a.Name.Contains(name));

            return await query.OrderBy(a => a.Name).Take(20).Cast<object>().ToListAsync();
        }

        public async Task<List<object>> GetBeneficiariesByAffiliate(string afiliadoId)
        {
            return await _limaContext.Beneficiarios
                .Where(b => b.idfaf == afiliadoId)
                .Select(b => new { id = b.codBenef, dni = "999999999", name = b.Name, relationship = b.codParent })
                .Cast<object>().ToListAsync();
        }

        public async Task<List<object>> GetWakes() => await _limaContext.Velatorios.Select(w => new { id = w.Id, name = w.Name }).OrderBy(w => w.name).Cast<object>().ToListAsync();

        public async Task<List<string>> GetRegions() => await _context.Ubigeos.Select(u => u.Region).Distinct().Where(r => r != null).OrderBy(r => r).ToListAsync();
        public async Task<List<string>> GetProvinces(string region) => await _context.Ubigeos.Where(u => u.Region == region).Select(u => u.Province).Distinct().Where(p => p != null).OrderBy(p => p).ToListAsync();
        public async Task<List<object>> GetDistricts(string region, string province) => await _context.Ubigeos.Where(u => u.Region == region && u.Province == province).Select(u => new { inei = u.Id, distrito = u.District }).OrderBy(d => d.distrito).Cast<object>().ToListAsync();

        public async Task<object?> GetBranchCapabilities(int branchId)
        {
            var branch = await _context.Filiales.FindAsync(branchId);
            if (branch == null) return null;
            return new { hasWake = branch.HasWakeService, hasCem = branch.HasOwnCemetery, branchName = branch.Name };
        }

        public async Task<List<object>> GetCemeteries(string? inei, int? branchId)
        {
            var query = _context.Cementerios.Where(c => c.IsActive);
            if (!string.IsNullOrEmpty(inei)) query = query.Where(c => c.UbigeoId == inei);
            else if (branchId.HasValue) query = query.Where(c => c.BranchId == branchId);
            return await query.Select(c => new { id = c.Id, name = c.Name, ruc = c.RUC, branchId = c.BranchId }).Cast<object>().ToListAsync();
        }

        public async Task<List<object>> GetStructures(int cemeteryId, string type) => await _context.SepulturasEstructura.Where(p => p.CemeteryId == cemeteryId && p.Type == type).Select(p => new { id = p.Id, name = p.Name }).OrderBy(p => p.name).Cast<object>().ToListAsync();
        public async Task<List<object>> GetSpaceMap(int structureId) => await _context.SepulturasNichos.Where(n => n.StructureId == structureId).Select(n => new { id = n.Id, rowLetter = n.RowLetter, columnNumber = n.ColumnNumber, status = n.Status }).OrderBy(n => n.rowLetter).ThenBy(n => n.columnNumber).Cast<object>().ToListAsync();

        public async Task<List<object>> GetAgencies(string ruc, string name, int? branchId)
        {
            if (branchId == null) return new List<object>();
            var query = _context.Agencias.Where(a => a.BranchId == branchId && a.IsActive);
            if (!string.IsNullOrEmpty(ruc)) query = query.Where(a => a.RUC.Contains(ruc));
            if (!string.IsNullOrEmpty(name)) query = query.Where(a => a.Name.Contains(name));
            return await query.Select(a => new { id = a.Id, ruc = a.RUC, name = a.Name, address = a.Address, phone = a.Phone }).Take(20).Cast<object>().ToListAsync();
        }

        public async Task<List<object>> GetAvailableVehicleTypesByBranch(int branchId) => await _context.Vehiculos.Include(v => v.VehicleType).Where(v => v.BranchId == branchId && v.IsActive).Select(v => new { id = v.VehicleType.Id, name = v.VehicleType.Name, icon = v.VehicleType.Icon }).Distinct().Cast<object>().ToListAsync();

        public async Task<string> GetBranchAbbreviation(int branchId)
        {
            var branch = await _context.Filiales.FirstOrDefaultAsync(f => f.Id == branchId);
            if (string.IsNullOrEmpty(branch?.UbigeoId)) return "GEN";
            string deptCode = branch.UbigeoId.Substring(0, 2);
            var ubigeo = await _context.Ubigeos.Where(u => u.Id.StartsWith(deptCode) && !string.IsNullOrEmpty(u.Abbreviation)).OrderBy(u => u.Id).FirstOrDefaultAsync();
            return ubigeo?.Abbreviation ?? "GEN";
        }


        // 1. Obtener Inventario (Stock) filtrado para Contratos
        public async Task<List<object>> GetStockItemsByBranch(int branchId)
        {
            return await _context.ProductosStock
                .Include(s => s.Product)
                    .ThenInclude(p => p.Category)
                .Include(s => s.Product)
                    .ThenInclude(p => p.SubCategory)
                .Where(s => s.BranchId == branchId &&
                            s.Product.ControlType == ControlType.Stock &&
                            s.Quantity > 0 &&
                            // --- FILTROS DE VISIBILIDAD ---
                            s.Product.IsAvailableForContract &&
                            s.Product.Category.ShowInContracts &&
                            s.Product.SubCategory.ShowInContracts)
                .Select(s => new {
                    id = s.ProductId,
                    name = s.Product.Name,
                    category = s.Product.Category.Name,
                    stock = s.Quantity
                })
                .ToListAsync()
                .ContinueWith(t => t.Result.Cast<object>().ToList());
        }

        // 2. Obtener Adicionales (Activos) filtrados para Contratos
        public async Task<List<object>> GetAvailableAssets(int branchId)
        {
            return await _context.ActivosFijos
                .Include(a => a.Product)
                    .ThenInclude(p => p.Category)
                .Include(a => a.Product)
                    .ThenInclude(p => p.SubCategory)
                .Where(a => a.BranchId == branchId &&
                            a.Status == AssetStatus.Available &&
                            a.Product.IsAvailableForContract &&
                            a.Product.Category.ShowInContracts &&
                            a.Product.SubCategory.ShowInContracts)
                .Select(a => new {
                    id = a.Id,
                    name = a.Product.Name,
                    category = a.Product.Category.Name,
                    patrimonialCode = a.PatrimonialCode
                })
                .ToListAsync()
                .ContinueWith(t => t.Result.Cast<object>().ToList());
        }

        private async Task<string> GenerateContractNumber(int branchId)
        {
            // 1. Obtener el prefijo de la sede (ej. LIM, ARE)
            string prefix = await GetBranchAbbreviation(branchId);
            int year = DateTime.Now.Year;

            // 2. Contar cuántos contratos existen en el año actual para esa sede
            // Usamos StartsWith para filtrar por el prefijo de la sede
            int count = await _context.Contratos
                .CountAsync(c => c.ContractNumber.StartsWith(prefix) &&
                                 c.CreatedAt.Year == year) + 1;

            // 3. Retornar el formato: PREFIX2026-00001
            return $"{prefix}{year}-{count:D5}";
        }

        public async Task<(bool success, string message, int contractId, string contractNumber)> CreateContract(ContractViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Generar Cabecera y Número de Contrato
                string contractNumber = await GenerateContractNumber(model.BranchId);

                var contract = new Contract
                {
                    ContractNumber = contractNumber,
                    BranchId = model.BranchId,
                    UbigeoId = model.Deceased.Inei,
                    AgencyId = model.AgencyId,

                    SolicitorDni = model.Solicitor.Dni,
                    SolicitorName = model.Solicitor.Name,
                    SolicitorType = model.Solicitor.Type, // <-- AGREGA ESTA LÍNEA

                    DeceasedDni = model.Deceased.Dni,
                    DeceasedName = model.Deceased.Name,
                    DeathDate = model.Deceased.DeathDate ,
                    BurialDate = model.Deceased.BurialDate,

                    // CONVERSIÓN DE STRING A TIMESPAN
                    BurialTime = string.IsNullOrEmpty(model.Deceased.BurialTime)
                        ? TimeSpan.Zero: TimeSpan.Parse(model.Deceased.BurialTime),

                    CemeteryId = model.Deceased.CemeteryId,
                    WakeId = model.Deceased.WakeId,
                    IntermentStructureId = model.Deceased.StructureId,
                    IntermentSpaceId = model.Deceased.IntermentSpaceId,

                    CreatedAt = DateTime.Now,
                    Status = "ACTIVO"
                };

                _context.Contratos.Add(contract);

                // Guardamos primero para que contract.Id se genere y esté disponible para los detalles
                await _context.SaveChangesAsync();

                // 2. Procesar Stock (Ataúdes y consumibles)
                if (model.StockItems != null)
                {
                    foreach (var prodId in model.StockItems)
                    {
                        var stock = await _context.ProductosStock
                            .FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.ProductId == prodId);

                        if (stock != null && stock.Quantity > 0)
                        {
                            stock.Quantity--; // Restamos del inventario de la sede
                            _context.DetallesProductosContrato.Add(new ContractProductDetail
                            {
                                ContractId = contract.Id,
                                ProductId = prodId,
                                Quantity = 1
                            });
                        }
                        else
                        {
                            throw new Exception($"No hay stock suficiente para el producto ID: {prodId}");
                        }
                    }
                }

                // 3. Procesar Activos (Capillas y equipos en préstamo)
                if (model.AssetItems != null)
                {
                    foreach (var assetId in model.AssetItems)
                    {
                        var asset = await _context.ActivosFijos.FindAsync(assetId);
                        if (asset != null)
                        {
                            asset.Status = AssetStatus.InUse; // Cambiamos a "En Uso"
                            _context.DetallesProductosContrato.Add(new ContractProductDetail
                            {
                                ContractId = contract.Id,
                                ProductId = asset.ProductId,
                                FixedAssetId = asset.Id,
                                Quantity = 1
                            });
                        }
                    }
                }

                // 4. Procesar Movilidad (Logística)
                if (model.MobilityItems != null)
                {
                    foreach (var vTypeId in model.MobilityItems)
                    {
                        _context.DetallesMovilidadContrato.Add(new ContractMovilityDetail
                        {
                            ContractId = contract.Id,
                            VehicleTypeId = vTypeId,
                            Status = "PENDIENTE"
                        });
                    }
                }

                // 5. MARCAR ESPACIO COMO OCUPADO
                if (model.Deceased.IntermentSpaceId.HasValue)
                {
                    var space = await _context.SepulturasNichos
                        .FindAsync(model.Deceased.IntermentSpaceId.Value);

                    if (space != null)
                    {
                        space.Status = IntermentStatus.Ocupado;
                        _context.SepulturasNichos.Update(space);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Contrato registrado con éxito", contract.Id, contractNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Obtenemos el mensaje de la excepción interna si existe para mayor detalle
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return (false, errorMsg, 0, "");
            }
        }
        public async Task<List<ContractListDto>> GetContractListAsync()
        {
            return await _context.Contratos
                .Include(c => c.Branch) // Traemos la data de la sede vinculada
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ContractListDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber,
                    DeceasedName = c.DeceasedName,
                    BurialDate = c.BurialDate,
                    BurialTime = c.BurialTime,
                    BranchName = c.Branch.Name,
                    Status = c.Status
                })
                .AsNoTracking()
                .ToListAsync();
        }

    }
}