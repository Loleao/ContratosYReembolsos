namespace ContratosYReembolsos.Models.ViewModels.Users
{
    public class ManageUserPermissionsViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string DNI { get; set; }
        public List<UserClaimViewModel> Claims { get; set; }
    }
}
