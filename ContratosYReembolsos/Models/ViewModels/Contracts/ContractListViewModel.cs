namespace ContratosYReembolsos.Models.ViewModels.Contracts
{
    public class ContractListViewModel
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public string DeceasedName { get; set; }
        // Aquí ya mandamos la fecha y hora lista para mostrar
        public string FullBurialDetail { get; set; }
        public string BranchName { get; set; }
        public string Status { get; set; }
    }
}