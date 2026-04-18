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

}
