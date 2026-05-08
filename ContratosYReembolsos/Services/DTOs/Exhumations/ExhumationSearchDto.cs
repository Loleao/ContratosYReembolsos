namespace ContratosYReembolsos.Services.DTOs.Exhumations
{
    public class ExhumationSearchDto
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string DeceasedName { get; set; }
        public string DeceasedDni { get; set; }
        public string BurialDate { get; set; }
        public string BranchName { get; set; }

        // Ubicación detallada para la lógica interna
        public string StructureName { get; set; }
        public string SpaceDetail { get; set; }

        // AGREGA ESTA LÍNEA: Es la que pide el compilador
        public string CurrentLocation { get; set; }

        public int? CurrentSpaceId { get; set; }
    }
}