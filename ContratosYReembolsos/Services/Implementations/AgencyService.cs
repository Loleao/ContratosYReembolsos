using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Agencies;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations.Agencies
{
    public class AgencyService : IAgencyService
    {
        private readonly ApplicationDbContext _context;

        public AgencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Branch>> GetBranchesWithAgencies()
            => await _context.Filiales.Include(b => b.Agencies).ToListAsync();

        public async Task<List<Agency>> GetAgenciesByBranch(int branchId)
            => await _context.Agencias.Where(a => a.BranchId == branchId).ToListAsync();

        public async Task<Agency?> GetById(int id) => await _context.Agencias.FindAsync(id);

        public async Task<string?> GetBranchName(int branchId)
        {
            var branch = await _context.Filiales.FindAsync(branchId);
            return branch?.Name;
        }

        public async Task<(bool success, string message)> Create(Agency model)
        {
            try
            {
                _context.Agencias.Add(model);
                await _context.SaveChangesAsync();
                return (true, "La agencia ha sido registrada correctamente.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool success, string message)> Edit(Agency model)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return (true, "Datos de la agencia actualizados correctamente.");
            }
            catch (Exception ex) { return (false, "Error en base de datos: " + ex.Message); }
        }

        public async Task<(bool success, string message)> ToggleStatus(int id, bool active)
        {
            var agency = await _context.Agencias.FindAsync(id);
            if (agency == null) return (false, "Agencia no encontrada.");

            agency.IsActive = active;
            await _context.SaveChangesAsync();
            return (true, active ? "La agencia ha sido activada." : "La agencia ha sido dada de baja.");
        }

        public async Task<(bool success, string message)> Delete(int id)
        {
            try
            {
                var agency = await _context.Agencias.FindAsync(id);
                if (agency == null) return (false, "Agencia no encontrada.");

                _context.Agencias.Remove(agency);
                await _context.SaveChangesAsync();
                return (true, "Convenio eliminado permanentemente.");
            }
            catch (Exception)
            {
                return (false, "No se puede eliminar: existen contratos vinculados a esta agencia.");
            }
        }
    }
}