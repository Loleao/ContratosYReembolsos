namespace ContratosYReembolsos.Models.ViewModels.Users
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; } // Agregado para mostrar el nombre de usuario (ej: pespejo)
        public string DNI { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string BranchName { get; set; }
    }
}