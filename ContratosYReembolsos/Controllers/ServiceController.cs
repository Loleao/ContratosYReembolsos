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

        [HttpGet]
        public async Task<IActionResult> GetWeeklyAvailability(int unitId, DateTime date)
        {
            try
            {
                // 1. Calculamos el lunes de la semana de la fecha enviada
                int daysUntilMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                DateTime startOfWeek = date.AddDays(-daysUntilMonday).Date;
                DateTime endOfWeek = startOfWeek.AddDays(7);

                // 2. Buscamos en ContractDetail los servicios programados para esa unidad en esa semana
                // Nota: Asegúrate de que el DbSet en tu Context se llame ContractDetails
                var occupiedSlots = await _context.DetallesContrato
                    .Where(d => d.ServiceId == unitId && // Aquí unitId es el Id del Servicio de Movilidad
                                d.ScheduledDate >= startOfWeek &&
                                d.ScheduledDate < endOfWeek)
                    .Select(d => new {
                        // Calculamos el índice: Lunes=0, Martes=1... Domingo=6
                        DayIndex = ((int)d.ScheduledDate.Value.DayOfWeek + 6) % 7,
                        Time = d.ScheduledTime.Value.ToString(@"hh\:mm")
                    })
                    .ToListAsync();

                return Json(occupiedSlots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}