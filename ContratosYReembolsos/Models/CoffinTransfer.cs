using System.ComponentModel.DataAnnotations.Schema;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Models
{
    public class CoffinTransfer
    {
        public int Id { get; set; }

        // Producto y cantidad
        public int CoffinVariantId { get; set; }
        public virtual CoffinVariant CoffinVariant { get; set; }
        public int Quantity { get; set; }

        public int OriginBranchId { get; set; }
        [ForeignKey("OriginBranchId")]
        public virtual Branch OriginBranch { get; set; }

        public int TargetBranchId { get; set; }
        [ForeignKey("TargetBranchId")]
        public virtual Branch TargetBranch { get; set; }

        public int? DepartureMovementId { get; set; }
        public virtual CoffinMovement? DepartureMovement { get; set; }

        // ID del movimiento "TRANSFERENCIA_IN" en el destino
        public int? ArrivalMovementId { get; set; }
        public virtual CoffinMovement? ArrivalMovement { get; set; }

        // --- ESTADO Y LOGÍSTICA ---
        public string Status { get; set; } = "EN_CAMINO"; // EN_CAMINO, RECIBIDO, RECHAZADO
        public string? GuiaRemision { get; set; }

        public DateTime DateSent { get; set; } = DateTime.Now;
        public string? SentBy { get; set; }

        public DateTime? DateReceived { get; set; }
        public string? ReceivedBy { get; set; }
        public string? ReceptionObservations { get; set; }
    }
}