using ContratosYReembolsos.Models.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace ContratosYReembolsos.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserListViewModel>> GetAllUsersAsync();
        Task<RegisterUserViewModel> GetUserForEditingAsync(string id);

        Task<IdentityResult> CreateUserAsync(RegisterUserViewModel model);
        Task<IdentityResult> UpdateUserAsync(RegisterUserViewModel model);

        Task<ManageUserPermissionsViewModel> GetUserForPermissionsAsync(string userId);
        Task<bool> UpdateUserPermissionsAsync(ManageUserPermissionsViewModel model);
    }
}
