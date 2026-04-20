namespace ContratosYReembolsos.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }
        public string IconClass { get; set; } = "fa-bell";
        public string? RequiredPermission { get; set; }
        
        public int? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
