namespace ContratosYReembolsos.Models.ViewModels.Users
{
    public class UserClaimViewModel
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public string Group { get; set; } // Para organizar la vista por secciones
        public bool IsSelected { get; set; }
    }
}
