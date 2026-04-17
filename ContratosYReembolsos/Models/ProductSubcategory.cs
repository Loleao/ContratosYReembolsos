namespace ContratosYReembolsos.Models
{
    public class ProductSubcategory
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public int CategoryId { get; set; }
        public virtual ProductCategory? Category { get; set; }
    }
}
