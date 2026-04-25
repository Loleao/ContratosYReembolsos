using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels.Users
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe tener exactamente 8 números")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nombre demasiado corto o largo")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; }

        public int? BranchId { get; set; }
        public IEnumerable<SelectListItem>? Branches { get; set; }
    }
}