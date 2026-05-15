using ContratosYReembolsos.Services.Interfaces;
using System.Security.Claims;

namespace ContratosYReembolsos.Services.Implementations
{
    // Services/Implementations/CurrentUserService.cs
    using System.Security.Claims;
    using ContratosYReembolsos.Services.Interfaces;

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public int? BranchId => int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst("BranchId")?.Value, out var id) ? id : null;

        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    }
}
