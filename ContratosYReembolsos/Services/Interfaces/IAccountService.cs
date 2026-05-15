namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(string userName, string password, bool rememberMe);
        Task LogoutAsync();
    }
}
