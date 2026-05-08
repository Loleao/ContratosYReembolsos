namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class ContractDetailDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string BranchName { get; set; }
        public string AgencyName { get; set; }

        public string SolicitorName { get; set; }
        public string SolicitorDni { get; set; }
        public string SolicitorType { get; set; }
        public string DeceasedName { get; set; }
        public string DeceasedDni { get; set; }

        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }
        public string CemeteryName { get; set; }
        public string UbigeoDetail { get; set; }
        public string FullLocation { get; set; }

        public List<string> Products { get; set; } = new List<string>();
        public List<string> Movilities { get; set; } = new List<string>();
        // NUEVOS CAMPOS
        public List<string> InternalServices { get; set; } = new List<string>();
        public List<string> ExternalServices { get; set; } = new List<string>();
    }
}