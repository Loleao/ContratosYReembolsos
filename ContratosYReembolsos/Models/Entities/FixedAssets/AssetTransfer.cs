using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;

namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetTransfer
    {
        public int Id { get; set; }
        public string InternalControlNumber { get; set; } // Ej: TR-ACT-2026-00001

        public int OriginBranchId { get; set; }
        public virtual Branch? OriginBranch { get; set; }

        public int TargetBranchId { get; set; }
        public virtual Branch? TargetBranch { get; set; }

        public TransferStatus Status { get; set; }

        public string? Observation { get; set; }
        public string? ReceptionObservation { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
        public string SentByUserId { get; set; }

        public DateTime? ReceivedAt { get; set; }
        public string? ReceivedByUserId { get; set; }

        public virtual ICollection<AssetTransferDetail> Details { get; set; } = new List<AssetTransferDetail>();
    }

    public enum TransferStatus 
    { 
        Sent, 
        Received, 
        Cancelled 
    }
}
