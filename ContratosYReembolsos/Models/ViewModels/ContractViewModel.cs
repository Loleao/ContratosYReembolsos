using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class ContractViewModel
    {
        // --- DATOS DEL CONTRATO (PROPIEDADES BÁSICAS) ---
        public int Id { get; set; }

        [Display(Name = "Número de Contrato")]
        public string ContractNumber { get; set; }

        [Required(ErrorMessage = "La filial es obligatoria")]
        public int BranchId { get; set; }
        public string BranchName { get; set; }

        // --- PASO 1: PERSONAS ---
        [Required(ErrorMessage = "DNI del solicitante requerido")]
        public string SolicitorDni { get; set; }
        public string SolicitorName { get; set; }
        public string SolicitorCip { get; set; }
        public string SolicitorType { get; set; } // Titular / Familiar

        [Required(ErrorMessage = "DNI del fallecido requerido")]
        public string DeceasedDni { get; set; }
        public string DeceasedName { get; set; }
        public DateTime DeathDate { get; set; } = DateTime.Now;

        // --- PASO 2: LOGÍSTICA ---
        public DateTime BurialDate { get; set; } = DateTime.Now;
        public TimeSpan BurialTime { get; set; } = DateTime.Now.TimeOfDay;
        public string UbigeoFull { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un espacio de sepultura")]
        public int IntermentSpaceId { get; set; }
        public string BurialDetail { get; set; } // Texto descriptivo para el contrato

        // --- PASO 3: SERVICIOS Y STOCK ---
        [Required(ErrorMessage = "Debe seleccionar un ataúd")]
        public int SelectedStockItemId { get; set; } // El ID del ataúd elegido

        // Lista de movilidades seleccionadas (Checkboxes)
        public List<string> SelectedMovilityTypes { get; set; } = new List<string>();

        // --- PASO 4: AGENCIA Y TOTAL ---
        [Required(ErrorMessage = "La agencia es obligatoria")]
        public int AgencyId { get; set; }
        public decimal TotalAmount { get; set; }

        // --- LISTAS PARA LLENAR LOS SELECTS (DROPDOWNS) ---
        public List<SelectListItem>? AvailableAgencies { get; set; }
        public List<SelectListItem>? AvailableCemeteries { get; set; }
        public List<SelectListItem>? AvailableCoffins { get; set; } // Ataúdes con stock
        public List<MovilityOptionViewModel> MovilityOptions { get; set; } = new List<MovilityOptionViewModel>();
    }

    // Clase auxiliar para los checkboxes de movilidad
    public class MovilityOptionViewModel
    {
        public string Type { get; set; } // RECOJO, CARROZA, etc.
        public string Icon { get; set; }
        public bool IsSelected { get; set; }
    }
}