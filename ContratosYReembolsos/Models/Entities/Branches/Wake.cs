namespace ContratosYReembolsos.Models.Entities.Branches
{
    public class Wake
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }

        public bool IsInternal { get; set; }

        public int BranchId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}