using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Hubs;
using ContratosYReembolsos.Models.Entities.Notifications;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateAsync(string title, string message, string permission, int? branchId, string url, string icon, string groupingKey = null)
        {
            // 1. VALIDACIÓN DE DUPLICADOS (Idempotencia)
            if (!string.IsNullOrEmpty(groupingKey))
            {
                // Si ya existe una notificación activa con esta misma clave, no hacemos nada
                bool exists = await _context.Notificaciones
                    .AnyAsync(n => n.GroupingKey == groupingKey && !n.IsRead);

                if (exists) return;
            }

            // 2. CREACIÓN DEL OBJETO
            var notification = new Notification
            {
                Title = title,
                Message = message,
                RequiredPermission = permission,
                BranchId = branchId,
                TargetUrl = url,
                IconClass = icon,
                GroupingKey = groupingKey, // Importante para el F5
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notificaciones.Add(notification);
            await _context.SaveChangesAsync();

            // 3. EMISIÓN EN TIEMPO REAL (SignalR)
            // Preparamos el objeto para el front-end
            var signalRData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                url = notification.TargetUrl,
                icon = notification.IconClass,
                timeAgo = "Ahora"
            };

            if (branchId.HasValue)
            {
                // Enviar solo a los usuarios conectados en esa sede específica
                await _hubContext.Clients.Group($"Branch_{branchId}")
                    .SendAsync("ReceiveNotification", signalRData);
            }
            else
            {
                // Si no hay sede, es global (para todos los Admins)
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", signalRData);
            }
        }

        public async Task<List<Notification>> GetActiveNotificationsAsync(IEnumerable<string> userClaims, int? userBranchId)
        {
            // Identificar si es Admin por sus claims
            bool isAdmin = userClaims.Any(c => c.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            var query = _context.Notificaciones.Where(n => !n.IsRead);

            if (!isAdmin)
            {
                // Filtro estricto para usuarios mortales
                query = query.Where(n =>
                    (n.RequiredPermission == null || userClaims.Contains(n.RequiredPermission)) &&
                    (n.BranchId == null || n.BranchId == userBranchId)
                );
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notificaciones.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}