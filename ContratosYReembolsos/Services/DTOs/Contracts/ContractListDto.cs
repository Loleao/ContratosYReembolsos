namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class ContractListDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string DeceasedName { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }
        public string BranchName { get; set; }
        public string Status { get; set; }
    }
}