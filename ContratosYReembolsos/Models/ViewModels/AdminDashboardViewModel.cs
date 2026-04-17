using ContratosYReembolsos.Models;
using static ContratosYReembolsos.Constants.Permissions;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // --- Métricas Rápidas (KPIs) ---
        public int TotalProducts { get; set; }     // Conteo de la tabla Products
        public int TotalAssets { get; set; }       // Conteo de la tabla FixedAssets
        public int PendingTransfers { get; set; }  // Traslados con estado "Sent" (En el aire)
        public int LowStockItems { get; set; }     // Alerta de productos con cantidad <= 5

        // --- Listados para la interfaz ---
        public List<Branch> Branches { get; set; } // Para el selector de sedes

        // --- Información de contexto ---  
        public string LastMovementDescription { get; set; } // Opcional: para mostrar actividad reciente
    }
}