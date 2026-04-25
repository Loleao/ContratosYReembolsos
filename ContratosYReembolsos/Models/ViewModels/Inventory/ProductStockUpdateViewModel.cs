namespace ContratosYReembolsos.Models.ViewModels.Inventory
{
    public class ProductStockUpdateViewModel
    {
        public int Id { get; set; } // El ID del ProductStock
        public decimal MinimumStock { get; set; } // El nuevo valor mínimo
    }
}
