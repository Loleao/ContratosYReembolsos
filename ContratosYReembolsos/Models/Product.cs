namespace ContratosYReembolsos.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; } // Código de referencia genérico
        public ControlType ControlType { get; set; }

        public int CategoryId { get; set; }
        public virtual ProductCategory? Category { get; set; }

        public int SubCategoryId { get; set; }
        public virtual ProductSubcategory? SubCategory { get; set; }
    }

    public enum ControlType
    {
        Stock,
        Asset 
    }
}
