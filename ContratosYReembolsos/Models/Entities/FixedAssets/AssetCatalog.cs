namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetCatalog
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: Laptop Lenovo ThinkPad E14
        public string Brand { get; set; } // Marca
        public string Model { get; set; } // Modelo
        public int SubcategoryId { get; set; }
        public AssetSubcategory? Subcategory { get; set; }

        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
