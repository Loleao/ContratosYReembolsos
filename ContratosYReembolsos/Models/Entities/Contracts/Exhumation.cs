namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class Exhumation
    {
        public int Id { get; set; }
        public string ExhumationNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public int OriginalContractId { get; set; }
        public virtual Contract? OriginalContract { get; set; }

        // ORIGEN (Snapshot)
        public int PreviousCemeteryId { get; set; }
        public int PreviousStructureId { get; set; }
        public int PreviousSpaceId { get; set; }
        public string PreviousLocationSnapshot { get; set; } // Ej: "PEC Sta Rosa - Pab. San Jose - A-10"

        // DESTINO
        public bool IsInternalRelocation { get; set; }
        public string DestinationDetails { get; set; } // Nombre cementerio externo o notas

        // DESTINO INTERNO (Snapshot si aplica)
        public int? NewCemeteryId { get; set; }
        public int? NewStructureId { get; set; }
        public int? NewSpaceId { get; set; }
        public string NewLocationSnapshot { get; set; } // Ej: "PEC Sta Rosa - Pab. Santa Ana - B-02"

        public decimal Cost { get; set; }
        public string Status { get; set; } // "Completado"
        public string MovementType { get; set; } // "REUBICACION", "TRASLADO", "ENTREGA"
    }
}