using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public int ServiceCategoryId { get; set; }
        public virtual ServiceCategory Category { get; set; }

        public string LogicType { get; set; }

        public virtual ICollection<StockItem> StockItems { get; set; }
    }
}
