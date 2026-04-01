namespace ContratosYReembolsos.Models.ViewModels
{
    public class InventoryViewModel
    {
        public int VariantId { get; set; }

        public string ModelName { get; set; }

        // Datos del "Padre" (CoffinVariant)
        public string Color { get; set; }
        public string Material { get; set; }
        public string Size { get; set; }
        public string? ImageUrl { get; set; }

        // Datos de la "Hija" (BranchStock)
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }

        // Propiedad calculada para mostrar en la Card de forma elegante
        public string FullDisplayName => $"{ModelName} ({Color})";
    }
}