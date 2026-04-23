using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Inventory;

namespace ContratosYReembolsos.Models.ViewModels
{
    public class InventoryDashboardViewModel
    {
        public int? SelectedBranchId { get; set; }
        public int? SelectedProductId { get; set; }
        public string SelectedPeriod { get; set; }
        public string DateValue { get; set; }

        public List<Branch> Sedes { get; set; }
        public List<Product> Productos { get; set; }

        public string GraficoLabelsJson { get; set; }
        public string ValoresConsumoJson { get; set; }
        public string ValoresIngresoJson { get; set; }

        public decimal TotalConsumo { get; set; }
        public decimal TotalIngreso { get; set; }

        public List<LineaConsumoViewModel> DetalleMovimientos { get; set; }
    }

    public class LineaConsumoViewModel
    {
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public decimal StockInicial { get; set; }
        public decimal TotalIngresos { get; set; } // Entradas + Recepciones
        public decimal TotalConsumo { get; set; }  // Salidas + Envíos
        public decimal StockFinal { get; set; }
    }
}
