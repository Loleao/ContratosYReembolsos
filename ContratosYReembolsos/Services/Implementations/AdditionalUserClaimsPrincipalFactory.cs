using System.Security.Claims;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ContratosYReembolsos.Services.Implementations
{
    public class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            // Genera los claims base (ID, Nombre de usuario, Roles)
            var identity = await base.GenerateClaimsAsync(user);

            // Inyectamos el BranchId para que tu CurrentUserService lo pueda leer
            identity.AddClaim(new Claim("BranchId", user.BranchId?.ToString() ?? "0"));

            // Opcional: Inyectamos el FullName para que no tengas que ir a la BD en el layout
            if (!string.IsNullOrEmpty(user.FullName))
            {
                identity.AddClaim(new Claim("FullName", user.FullName));
            }

            return identity;
        }
    }
}