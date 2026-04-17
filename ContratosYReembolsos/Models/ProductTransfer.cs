using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Models
{
    [Index(nameof(InternalControlNumber), IsUnique = true)]
    public class ProductTransfer
    {
        public int Id { get; set; }
        public string InternalControlNumber { get; set; }
        public int OriginBranchId { get; set; }
        public virtual Branch? OriginBranch { get; set; }
        public int TargetBranchId { get; set; }
        public virtual Branch? TargetBranch { get; set; }

        public TransferStatus Status { get; set; }

        public DateTime SentAt { get; set; }
        public DateTime? ReceivedAt { get; set; }

        public string SentByUserId { get; set; }
        public string? ReceivedByUserId { get; set; }

        public string? Observation { get; set; }
        public string? ReceptionObservation { get; set; } // Muy útil para el sustento legal

        public virtual ICollection<ProductTransferDetail> Details { get; set; }
    }

    public enum TransferStatus
    {
        [Display(Name = "Creado")]
        Created,    // Borrador
        [Display(Name = "Enviado")]
        Sent,       // Salió del almacén (En el aire)
        [Display(Name = "Recibido")]
        Received,   // Llegó a destino
        [Display(Name = "Cancelado")]
        Cancelled   // Se anuló el movimiento
    }
}
