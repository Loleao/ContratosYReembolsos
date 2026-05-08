namespace ContratosYReembolsos.Models.ViewModels.Exhumations
{
    public class ExhumationHistoryItemViewModel
    {
        public int ExhumationId { get; set; } 
        public string ExhumationNumber { get; set; }
        public string RequestDate { get; set; }
        public string MovementType { get; set; } // REUBICACION, TRASLADO, etc.

        // Comparativa de ubicaciones
        public string PreviousLocation { get; set; }
        public string NewLocation { get; set; }

        public decimal Cost { get; set; }
    }
}
