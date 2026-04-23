namespace ContratosYReembolsos.Models.Entities.Notifications
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? TargetUrl { get; set; }
        public string? IconClass { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public int? BranchId { get; set; } // Null = Global (Admin)
        public string? RequiredPermission { get; set; }

        // Clave para evitar duplicados (ej: "STOCK_10_SED1")
        public string? GroupingKey { get; set; }
    }
}
