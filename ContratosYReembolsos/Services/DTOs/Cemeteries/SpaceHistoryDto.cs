namespace ContratosYReembolsos.Services.DTOs.Cemeteries
{
    public class SpaceHistoryDto
    {
        public string ContractNumber { get; set; } = string.Empty;
        public string DeceasedName { get; set; } = string.Empty;
        public string DeceasedDni { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string Observations { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }

    public class SpaceHistoryDetailDto
    {
        public int SpaceId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string RowLetter { get; set; } = string.Empty;
        public int ColumnNumber { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string StructureName { get; set; } = string.Empty;
        public string StructureType { get; set; } = string.Empty;
        public string CemeteryName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        // Colección de movimientos cronológicos
        public List<SpaceHistoryDto> Timeline { get; set; } = new();
    }
}