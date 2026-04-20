using Microsoft.AspNetCore.Mvc;
using ContratosYReembolsos.Services;
using ContratosYReembolsos.Data;
using Microsoft.EntityFrameworkCore; // Ajusta según tu namespace

namespace ContratosYReembolsos.Controllers
{
    // Es vital que el nombre de la clase termine en "Controller"
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public NotificationController(INotificationService notificationService, ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        // Esta es la acción que busca el JS
        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            // Obtenemos todos los valores de los claims (permisos y roles)
            var allUserClaims = User.Claims.Select(c => c.Value).ToList();

            var branchClaim = User.FindFirst("BranchId")?.Value;
            int? branchId = string.IsNullOrEmpty(branchClaim) ? null : int.Parse(branchClaim);

            // DETERMINAR SI REVISAMOS STOCK (Proactivo)
            // Usamos IsInRole directamente aquí porque tenemos acceso al User de Identity
            bool isAdmin = User.IsInRole("Admin");

            if (isAdmin || branchId.HasValue)
            {
                var stockQuery = _context.ProductosStock.Include(ps => ps.Product).AsQueryable();

                // Si no es admin, filtramos solo su sede. Si es admin, revisa TODO el inventario nacional.
                if (!isAdmin)
                {
                    stockQuery = stockQuery.Where(ps => ps.BranchId == branchId);
                }

                var lowStockItems = await stockQuery
                    .Where(ps => ps.Quantity <= ps.MinimumStock)
                    .ToListAsync();

                foreach (var item in lowStockItems)
                {
                    // Evitar duplicados
                    bool alreadyNotified = await _context.Notificaciones
                        .AnyAsync(n => n.BranchId == item.BranchId && !n.IsRead && n.Title.Contains(item.Product.Name));

                    if (!alreadyNotified)
                    {
                        // El Admin genera la notificación para la sede correspondiente
                        await _notificationService.CreateAsync(
                            "Bajo Stock detectado",
                            $"El producto {item.Product.Name} en la sede {item.BranchId} requiere reposición.",
                            "Inventario.Ver",
                            item.BranchId,
                            "/Inventory/Stock",
                            "fa-triangle-exclamation"
                        );
                    }
                }
            }

            // El servicio ahora recibirá el claim de "Admin" y sabrá que no debe filtrar
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
                    timeAgo = "Reciente"
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