namespace ContratosYReembolsos.Models
{
    public class CoffinTransfer
    {
        public int Id { get; set; }

        // Producto y cantidad
        public int CoffinVariantId { get; set; }
        public virtual CoffinVariant CoffinVariant { get; set; }
        public int Quantity { get; set; }

        // Ruta
        public string OriginSubsidiaryId { get; set; } // "070000" (Lima)
        public string TargetSubsidiaryId { get; set; } // "100502" (Andahuaylas)

        // --- RASTREO DE MOVIMIENTOS (Tu idea de los 2 campos) ---

        // ID del movimiento "TRANSFERENCIA_OUT" en el origen
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