namespace ContratosYReembolsos.Models.ViewModels
{
    public class ConsumoDashboardViewModel
    {
        public DateTime MesFiltro { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; } // "Global" o Nombre de Sede

        // Tarjetas de Resumen
        public int ConsumoTotalMes { get; set; }
        public string TopProducto { get; set; }
        public decimal VariacionAnterior { get; set; }

        // Para el Gráfico de Tendencia (JSON para Chart.js)
        public string GraficoTendenciaDataJson { get; set; }

        // Para la Tabla de Detalle
        public List<LineaConsumoViewModel> DetalleItems { get; set; }
    }

    public class LineaConsumoViewModel
    {
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public int StockInicial { get; set; }
        public int TotalCompras { get; set; }
        public int TotalTraslados { get; set; }
        public int TotalConsumo { get; set; }
        public int StockFinal { get; set; }
    }
}
