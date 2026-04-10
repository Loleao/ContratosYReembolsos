using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Data; // Tu ApplicationDbContext

namespace ContratosYReembolsos.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GetUnitPicker()
        {
            return PartialView("_UnitAvailabilityPicker");
        }

        [HttpGet]
        public async Task<IActionResult> GetServices(string burialType)
        {
            var services = await _context.Servicios // Asegúrate que el DbSet sea Services
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .Select(s => new {
                    s.Id,
                    s.Name,
                    s.Description,
                    CategoryName = s.Category.Name,
                    // CAMBIO CLAVE: Enviamos el LogicType al Frontend
                    LogicType = s.LogicType,
                    // Esto servirá para mostrar stock en ataúdes
                    CurrentStock = s.StockItems.Count(si => si.Status == "Disponible")
                })
                .ToListAsync();

            return Json(services);
        }

        // Nueva acción para obtener series de ataúdes (Placeholder para tu futuro gestor)
        [HttpGet]
        public async Task<IActionResult> GetStockByService(int serviceId)
        {
            var stock = await _context.StockItems
                .Where(si => si.ServiceId == serviceId && si.Status == "Disponible")
                .Select(si => new { si.Id, si.SerialNumber })
                .ToListAsync();
            return Json(stock);
        }
    }
}