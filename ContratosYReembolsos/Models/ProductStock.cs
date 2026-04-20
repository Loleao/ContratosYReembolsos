namespace ContratosYReembolsos.Models
{
    public class ProductStock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int BranchId { get; set; }
        public decimal Quantity { get; set; } 
        public decimal MinimumStock { get; set; }
    }
}
