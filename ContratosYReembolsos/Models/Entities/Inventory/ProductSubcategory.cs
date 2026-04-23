namespace ContratosYReembolsos.Models.Entities.Inventory
{
    public class ProductSubcategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ShowInContracts { get; set; } = true;
        public int CategoryId { get; set; }
        public virtual ProductCategory? Category { get; set; }
    }
}
