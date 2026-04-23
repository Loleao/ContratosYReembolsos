using ContratosYReembolsos.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class SubsidiaryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;

        public SubsidiaryController(ApplicationDbContext context, LimaContractsDbContext limaContext)
        {
            _context = context;
            _limaContext = limaContext;
        }

        // Listado de todas las filiales
        public async Task<IActionResult> Index()
        {
            var filiales = await _limaContext.Filiales.OrderBy(f => f.Name).ToListAsync();
            return View(filiales);
        }

        public async Task<IActionResult> Inventory(int id)
        {
            var filial = await _limaContext.Filiales.FindAsync(id);
            if (filial == null) return NotFound();

            ViewBag.FilialName = filial.Name;
            ViewBag.FilialId = filial.Id;

            // Buscamos los stocks que pertenecen a esta filial
            var stock = await _context.StockFilial
                .Include(s => s.CoffinVariant)
                .Where(s => s.BranchId == id)
                .ToListAsync();

            return View(stock);
        }
    }
}
