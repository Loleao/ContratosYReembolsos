using Microsoft.AspNetCore.SignalR;

namespace ContratosYReembolsos.Hubs
{
    public class NotificationHub : Hub
    {
        // Método para que el cliente se una a un grupo basado en su BranchId
        public async Task JoinBranchGroup(string branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, branchId);
        }

        // Método para unirse a un grupo global (opcional)
        public async Task JoinGlobalGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Global");
        }
    }
}