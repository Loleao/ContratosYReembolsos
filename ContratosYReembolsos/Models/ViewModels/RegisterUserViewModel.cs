using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "DNI debe tener 8 dígitos")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "Nombre completo es obligatorio")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare("Password", ErrorMessage = "No coinciden")]
        public string ConfirmPassword { get; set; }

        public int? BranchId { get; set; } // El amarre
        public IEnumerable<SelectListItem>? Branches { get; set; } // El combo
    }
}