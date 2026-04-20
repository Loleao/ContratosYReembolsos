namespace ContratosYReembolsos.Services
{
    public interface INotificationService
    {
        Task CreateAsync(string title, string message, string permission, int? branchId = null, string? url = null, string icon = "fa-bell");

        Task<List<Models.Notification>> GetActiveNotificationsAsync(IEnumerable<string> userPermissions, int? userBranchId);

        // Marca como leída
        Task MarkAsReadAsync(int notificationId);
    }
}