namespace ContratosYReembolsos.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        int? BranchId { get; }
        bool IsAdmin { get; }
    }
}
