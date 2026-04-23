using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Models.ViewModels;
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

        public async Task<List<object>> GetCoffinsByBranch(int branchId) => await _context.StockFilial.Include(s => s.CoffinVariant).ThenInclude(v => v.Coffin).Where(s => s.BranchId == branchId && s.Quantity > 0).Select(s => new { id = s.CoffinVariantId, name = $"{s.CoffinVariant.Coffin.ModelName} - {s.CoffinVariant.Color}", stock = s.Quantity, image = s.CoffinVariant.ImageUrl }).Cast<object>().ToListAsync();
        public async Task<List<object>> GetAvailableVehicleTypesByBranch(int branchId) => await _context.Vehiculos.Include(v => v.VehicleType).Where(v => v.BranchId == branchId && v.IsActive).Select(v => new { id = v.VehicleType.Id, name = v.VehicleType.Name, icon = v.VehicleType.Icon }).Distinct().Cast<object>().ToListAsync();

        public async Task<string> GetBranchAbbreviation(int branchId)
        {
            var branch = await _context.Filiales.FirstOrDefaultAsync(f => f.Id == branchId);
            if (string.IsNullOrEmpty(branch?.UbigeoId)) return "GEN";
            string deptCode = branch.UbigeoId.Substring(0, 2);
            var ubigeo = await _context.Ubigeos.Where(u => u.Id.StartsWith(deptCode) && !string.IsNullOrEmpty(u.Abbreviation)).OrderBy(u => u.Id).FirstOrDefaultAsync();
            return ubigeo?.Abbreviation ?? "GEN";
        }

        public async Task<(bool success, string message, int contractId, string contractNumber)> CreateContract(ContractViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string prefix = await GetBranchAbbreviation(model.BranchId);
                int year = DateTime.Now.Year;
                int count = await _context.Contratos.CountAsync(c => c.ContractNumber.StartsWith(prefix) && c.CreatedAt.Year == year) + 1;
                string contractNumber = $"{prefix}{year}-{count:D5}";

                var contract = new Contract
                {
                    ContractNumber = contractNumber,
                    CreatedAt = DateTime.Now,
                    Status = "ACTIVO",
                    BranchId = model.BranchId,
                    SolicitorDni = model.Solicitor.Dni,
                    SolicitorName = model.Solicitor.Name,
                    SolicitorType = model.Solicitor.Type,
                    DeceasedDni = model.Deceased.Dni,
                    DeceasedName = model.Deceased.Name,
                    DeathDate = model.Deceased.DeathDate,
                    BurialDate = model.Deceased.BurialDate,
                    BurialTime = TimeSpan.Parse(model.Deceased.BurialTime),
                    UbigeoId = model.Deceased.Inei,
                    WakeId = model.Deceased.WakeId > 0 ? model.Deceased.WakeId : null,
                    CemeteryId = model.Deceased.CemeteryId,
                    IntermentStructureId = model.Deceased.StructureId > 0 ? model.Deceased.StructureId : null,
                    IntermentSpaceId = model.Deceased.IntermentSpaceId > 0 ? model.Deceased.IntermentSpaceId : null,
                    CoffinVariantId = model.CoffinVariantId,
                    AgencyId = model.AgencyId,
                    TotalAmount = model.TotalAmount
                };

                _context.Contratos.Add(contract); await _context.SaveChangesAsync();

                if (model.RequiredVehicles != null)
                {
                    foreach (var vId in model.RequiredVehicles)
                        _context.DetallesMovilidadContrato.Add(new ContractMovilityDetail { ContractId = contract.Id, VehicleTypeId = vId, Status = "PENDIENTE" });
                }

                if (contract.IntermentSpaceId.HasValue)
                {
                    var space = await _context.SepulturasNichos.FindAsync(contract.IntermentSpaceId);
                    if (space != null) { space.Status = IntermentStatus.Ocupado; _context.Update(space); }
                }

                var stock = await _context.StockFilial.FirstOrDefaultAsync(s => s.BranchId == model.BranchId && s.CoffinVariantId == model.CoffinVariantId);
                if (stock == null || stock.Quantity <= 0) throw new Exception("Sin stock de ataúd.");
                stock.Quantity--; _context.Update(stock);

                await _context.SaveChangesAsync(); await transaction.CommitAsync();
                return (true, "OK", contract.Id, contractNumber);
            }
            catch (Exception ex) { await transaction.RollbackAsync(); return (false, ex.Message, 0, ""); }
        }

        public async Task<Contract?> GetContractForPdf(int id) => await _context.Contratos.Include(c => c.Branch).Include(c => c.CoffinVariant).ThenInclude(v => v.Coffin).Include(c => c.Cemetery).Include(c => c.MovilityDetails).ThenInclude(m => m.VehicleType).FirstOrDefaultAsync(c => c.Id == id);
    }
}