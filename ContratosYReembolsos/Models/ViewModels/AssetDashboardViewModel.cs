namespace ContratosYReembolsos.Models.ViewModels
{
    public class AssetDashboardViewModel
    {
        // Filtros y Estado
        public int? SelectedBranchId { get; set; }
        public string SelectedPeriod { get; set; }
        public string DateValue { get; set; }

        // Catálogos
        public List<Branch> Sedes { get; set; }

        // Datos del Gráfico Circular (Distribución de Estados)
        public string EstadosLabelsJson { get; set; }
        public string EstadosValoresJson { get; set; }

        // Datos del Gráfico de Líneas (Altas vs Asignaciones)
        public string GraficoLabelsJson { get; set; }
        public string ValoresAltasJson { get; set; } // Ingresos de activos
        public string ValoresBajasJson { get; set; } // Salidas/Bajas de activos

        // Datos de la Tabla
        public List<ResumenActivoViewModel> ResumenActivos { get; set; }
    }

    public class ResumenActivoViewModel
    {
        public string ProductName { get; set; }
        public int Total { get; set; }
        public int Disponibles { get; set; }
        public int Asignados { get; set; }
        public int EnMantenimiento { get; set; }
    }
}
