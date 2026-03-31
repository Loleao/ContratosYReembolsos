namespace ContratosYReembolsos.ViewModels
{
    public class ServiceSelectionViewModel
    {
        public int ServiceId { get; set; }

        // Este campo es el secreto del éxito:
        // Si es un ataúd, vendrá null (solo resta stock del ServiceId).
        // Si es una carroza, vendrá con el ID de la "Carroza 01" (para la agenda).
        public int? PhysicalUnitId { get; set; }

        public string CategoryName { get; set; } // Útil para validaciones rápidas en el servidor
    }
}