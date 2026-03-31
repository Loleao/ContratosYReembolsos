namespace ContratosYReembolsos.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public bool IsInventoryTracked { get; set; }
        public bool RequiresScheduleValidation { get; set; } 

        public virtual ICollection<Service> Services { get; set; }
    }
}
