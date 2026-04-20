using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services
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

        public async Task CreateAsync(string title, string message, string permission, int? branchId = null, string? url = null, string icon = "fa-bell")
        {
            // 1. Persistencia en Base de Datos (HTTP/SQL)
            var notification = new Models.Notification
            {
                Title = title,
                Message = message,
                RequiredPermission = permission,
                BranchId = branchId,
                TargetUrl = url,
                IconClass = icon,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notificaciones.Add(notification);
            await _context.SaveChangesAsync();

            // 2. Notificación en Tiempo Real (SignalR/WebSockets)
            if (branchId.HasValue)
            {
                // Solo enviamos a los usuarios que pertenecen al grupo de esa sede
                await _hubContext.Clients.Group(branchId.Value.ToString()).SendAsync("ReceiveNotification");
            }
            else
            {
                // Si es global (branchId null), enviamos a todos los conectados
                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
            }
        }

        public async Task<List<Models.Notification>> GetActiveNotificationsAsync(IEnumerable<string> userClaims, int? userBranchId)
        {
            // Buscamos si entre los claims existe el valor "Admin" 
            // Identity guarda el rol en un claim específico, así que verificamos ambos casos
            bool isAdmin = userClaims.Any(c => c.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            var query = _context.Notificaciones.Where(n => !n.IsRead);

            if (!isAdmin)
            {
                // Si NO es admin, aplicamos el filtro de Branch y Permisos
                query = query.Where(n =>
                    (n.RequiredPermission == null || userClaims.Contains(n.RequiredPermission)) &&
                    (n.BranchId == null || n.BranchId == userBranchId)
                );
            }
            // Si ES admin, se salta los filtros anteriores y ve todas las sedes

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