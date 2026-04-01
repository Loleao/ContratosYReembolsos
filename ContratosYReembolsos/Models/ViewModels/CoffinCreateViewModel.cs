using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class CoffinCreateViewModel
    {
        public string ModelName { get; set; }

        public string Color { get; set; }
        public string Material { get; set; }
        public string Size { get; set; }

        [Range(0, 99999)]
        public int InitialStock { get; set; }

        public int MinimumStock { get; set; }
    }
}
