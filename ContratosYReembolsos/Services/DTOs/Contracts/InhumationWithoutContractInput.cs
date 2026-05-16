using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class InhumationWithoutContractInput
    {
        [Required(ErrorMessage = "El nombre del fallecido es obligatorio.")]
        [StringLength(150)]
        public string DeceasedName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(20)]
        public string DeceasedDni { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de defunción es obligatoria.")]
        public DateTime DeathDate { get; set; }

        [Required(ErrorMessage = "La fecha de sepelio es obligatoria.")]
        public DateTime BurialDate { get; set; }

        public int CemeteryId { get; set; }
        public int? StructureId { get; set; }
        public int? IntermentSpaceId { get; set; }

        public string? Observations { get; set; }
    }
}
