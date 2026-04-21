using Microsoft.AspNetCore.Mvc;
using ContratosYReembolsos.Services;
using ContratosYReembolsos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure; // Ajusta según tu namespace

namespace ContratosYReembolsos.Controllers
{
    // Es vital que el nombre de la clase termine en "Controller"
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public NotificationController(INotificationService notificationService, ApplicationDbContext context, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _notificationService = notificationService;
            _context = context;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        // Acción para ver el historial completo
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branchClaim = User.FindFirst("BranchId")?.Value;
            int? branchId = string.IsNullOrEmpty(branchClaim) ? null : int.Parse(branchClaim);
            bool isAdmin = User.IsInRole("Admin");

            var query = _context.Notificaciones.AsQueryable();

            // Si no es admin, solo ve las de su sede
            if (!isAdmin)
            {
                query = query.Where(n => n.BranchId == null || n.BranchId == branchId);
            }

            var history = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(history);
        }


        // Esta es la acción que busca el JS
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var allUserClaims = User.Claims.Select(c => c.Value).ToList();
            var branchClaim = User.FindFirst("BranchId")?.Value;
            int? branchId = string.IsNullOrEmpty(branchClaim) ? null : int.Parse(branchClaim);
            bool isAdmin = User.IsInRole("Admin");

            if (isAdmin || branchId.HasValue)
            {
                var stockQuery = _context.ProductosStock.Include(ps => ps.Product).AsQueryable();

                if (!isAdmin)
                {
                    stockQuery = stockQuery.Where(ps => ps.BranchId == branchId);
                }

                var lowStockItems = await stockQuery
                    .Where(ps => ps.Quantity <= ps.MinimumStock)
                    .ToListAsync();

                foreach (var item in lowStockItems)
                {
                    // 1. Definimos la llave única
                    string gKey = $"LOW_STOCK_{item.ProductId}_{item.BranchId}";

                    // 2. Generamos la URL dinámica correcta
                    string targetUrl = Url.Action("Inventory", "Inventory", new { branchId = item.BranchId });

                    // 3. PASAMOS la gKey al servicio (Era el parámetro que faltaba)
                    await _notificationService.CreateAsync(
                        "Bajo Stock detectado",
                        $"El producto {item.Product.Name} requiere reposición.",
                        "Inventario.Ver",
                        item.BranchId,
                        targetUrl,
                        "fa-triangle-exclamation",
                        gKey // <--- ¡AQUÍ ESTÁ LA CORRECCIÓN!
                    );
                }
            }

            var notifications = await _notificationService.GetActiveNotificationsAsync(allUserClaims, branchId);

            return Json(new
            {
                count = notifications.Count,
                items = notifications.Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    iconClass = n.IconClass,
                    targetUrl = n.TargetUrl,
                    timeAgo = GetRelativeTime(n.CreatedAt) // Usamos tu función de tiempo
                })
            });
        }

        // Función auxiliar opcional para el tiempo relativo
        private string GetRelativeTime(DateTime date)
        {
            var ts = DateTime.Now - date;
            if (ts.TotalMinutes < 1) return "Ahora mismo";
            if (ts.TotalMinutes < 60) return $"Hace {Math.Floor(ts.TotalMinutes)} min";
            if (ts.TotalHours < 24) return $"Hace {Math.Floor(ts.TotalHours)} horas";
            return date.ToString("dd/MM/yyyy");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }
    }
}