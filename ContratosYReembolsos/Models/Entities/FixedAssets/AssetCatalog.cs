namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetCatalog
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: Laptop Lenovo ThinkPad E14
        public string Brand { get; set; } // Marca
        public string Model { get; set; } // Modelo

        public int CategoryId { get; set; }
        public virtual AssetCategory? Category { get; set; }

        public int SubcategoryId { get; set; }
        public virtual AssetSubcategory? Subcategory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
