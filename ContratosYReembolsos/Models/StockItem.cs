using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class StockItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string SubsidiaryId { get; set; }
    }
}
