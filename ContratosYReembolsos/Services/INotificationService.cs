using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string title, string message, string permission, int? branchId, string url, string icon, string groupingKey = null);
        Task<List<Notification>> GetActiveNotificationsAsync(IEnumerable<string> userClaims, int? userBranchId);
        Task MarkAsReadAsync(int notificationId);
    }
}