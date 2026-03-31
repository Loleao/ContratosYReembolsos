using System;
using System.Collections.Generic;

namespace ContratosYReembolsos.ViewModels
{
    public class ContractViewModel
    {
        // --- PASO 1: SOLICITANTE ---
        public int SolicitorId { get; set; } // ID en base de datos si existe
        public string SolicitorDni { get; set; }
        public string SolicitorName { get; set; }
        public string SolicitorCip { get; set; }
        public string SolicitorType { get; set; } // "Titular" o "Familiar"

        // --- PASO 2: DIFUNTO ---
        public int DeceasedId { get; set; }
        public string DeceasedDni { get; set; }
        public string DeceasedName { get; set; }
        public DateTime DeathDate { get; set; }

        // UBICACIÓN Y LOGÍSTICA DE SEPULCRO
        public DateTime BurialDate { get; set; }
        public string BurialTime { get; set; } // Lo recibimos como string "15:30" desde el input type="time"
        public string CemeteryId { get; set; }
        public string BurialType { get; set; } // "Pabellon", "Tumba", "Columbario"
        public string BurialDetail { get; set; } // El texto del mapa (Fila, Nro, etc)
        public int? NicheId { get; set; } // ID del nicho si se seleccionó del mapa

        // UBICACIÓN DE FALLECIMIENTO (UBIGEO)
        public string Inei { get; set; }
        public string UbigeoFull { get; set; }
        public int WakeId { get; set; } // ID del local de velatorio

        // --- PASO 3: AGENCIA ---
        public int AgencyId { get; set; }

        // --- PASO 4: LISTA DE SERVICIOS SELECCIONADOS ---
        // Aquí es donde enviamos la lista de lo que el usuario marcó en las cards
        public List<ServiceSelectionViewModel> SelectedServices { get; set; } = new List<ServiceSelectionViewModel>();
    }
}