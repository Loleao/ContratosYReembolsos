using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;

namespace ContratosYReembolsos.Models.ViewModels.Inventory
{
    public class StockEntryFormViewModel
    {
        public List<Product> StockProducts { get; set; } = new();
        public List<Product> AssetProducts { get; set; } = new();
        public List<Branch> Branches { get; set; } = new();

        public string ServerDate => DateTime.Now.ToString("dd/MM/yyyy");
    }
}