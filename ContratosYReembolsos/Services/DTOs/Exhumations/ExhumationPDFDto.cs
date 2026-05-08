namespace ContratosYReembolsos.Services.DTOs.Exhumations
{
    public class ExhumationPDFDto
    {
        public string ExhumationNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string DeceasedName { get; set; }
        public string DeceasedDni { get; set; }
        public string OriginLocation { get; set; }
        public string DestinationLocation { get; set; }
        public string MovementType { get; set; }
        public decimal TotalCost { get; set; }
        public string Observations { get; set; }
    }
}
