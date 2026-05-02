namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class Exhumation
    {
        public int Id { get; set; }
        public string ExhumationNumber { get; set; } // Formato: EX-2026-0001
        public DateTime RequestDate { get; set; }

        // Referencia al contrato donde está el fallecido actualmente
        public int OriginalContractId { get; set; }
        public virtual Contract OriginalContract { get; set; }

        // Datos del Destino
        public bool IsInternalRelocation { get; set; } // true: FONAFUN, false: Externo
        public string DestinationDetails { get; set; } // Nombre del cementerio externo o notas

        // Si es interno, apuntamos a nuestra infraestructura
        public int? NewCemeteryId { get; set; }
        public int? NewIntermentStructureId { get; set; }
        public int? NewIntermentSpaceId { get; set; }

        public decimal Cost { get; set; } // Costo del nuevo espacio (si aplica)
        public string Status { get; set; } // "Pendiente", "En Traslado", "Completado"
        public string Observations { get; set; }
    }
}