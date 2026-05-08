namespace ContratosYReembolsos.Services.DTOs.Exhumations
{
    public class ExhumationCreateDto
    {
        public int ContractId { get; set; }
        public DateTime ExhumationDate { get; set; }
        public string DestinationType { get; set; }
        public int? CemeteryId { get; set; }
        public string CemeteryName { get; set; }
        public bool IsInternal { get; set; }

        // Añadimos StructureId para el historial
        public int? NewStructureId { get; set; }
        public int? NewIntermentSpaceId { get; set; }

        // Snapshot de texto que viene del JS
        public string NewLocationName { get; set; }

        public string Observations { get; set; }
        public decimal TotalCost { get; set; }
    }
}