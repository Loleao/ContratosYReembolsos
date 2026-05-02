namespace ContratosYReembolsos.Services.DTOs.Exhumations
{
    public class ExhumationCreateDto
    {
        public int OriginalContractId { get; set; } // Seleccionado al buscar el fallecido
        public DateTime RequestDate { get; set; } = DateTime.Now;

        public bool IsInternalRelocation { get; set; }
        public int? NewCemeteryId { get; set; }
        public int? NewIntermentStructureId { get; set; }
        public int? NewIntermentSpaceId { get; set; }

        public string ExternalDestination { get; set; }
        public string Observations { get; set; }

        // El costo se calculará en el Service, pero lo mostramos en el UI
        public decimal RelocationCost { get; set; }
    }
}