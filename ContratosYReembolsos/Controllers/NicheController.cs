using ContratosYReembolsos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class NicheController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LimaContractsDbContext _limaContext;

        public NicheController(ApplicationDbContext context, LimaContractsDbContext limaContext)
        {
            _context = context;
            _limaContext = limaContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPavilions(string cemeteryId, string type)
        {
            var pavilions = await _context.Pabellones
                .Where(p => p.CemeteryId == cemeteryId && p.Type == type)
                .Select(p => new { id = p.Id, name = p.Name })
                .OrderBy(p => p.name)
                .ToListAsync();

            return Json(pavilions);
        }

        [HttpGet]
        public async Task<IActionResult> GetMap(string cemeteryId, int pavilionId)
        {
            var niches = await _context.Nichos
                .Where(n => n.CemeteryId == cemeteryId && n.PavilionId == pavilionId)
                .OrderBy(n => n.Row).ThenBy(n => n.Column)
                .ToListAsync();

            return Json(niches);
        }
    }
}
