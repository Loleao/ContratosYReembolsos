namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class ContractReportDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string CreatedAt { get; set; }
        public string BranchName { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }

        // Solicitante
        public string SolicitorName { get; set; }
        public string SolicitorDni { get; set; }
        public string SolicitorType { get; set; }

        // Fallecido
        public string DeceasedName { get; set; }
        public string DeceasedDni { get; set; }
        public string DeathDate { get; set; }

        // Logística
        public string BurialDate { get; set; }
        public string BurialTime { get; set; }
        public string UbigeoFull { get; set; }
        public string CemeteryName { get; set; }
        public string BurialLocationDetail { get; set; } // "Pabellón X - Fila Y..."

        // Items (Ataúd, Capilla, Movilidad)
        public List<OrderItemDto> Services { get; set; }
    }

    public class OrderItemDto
    {
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }
}