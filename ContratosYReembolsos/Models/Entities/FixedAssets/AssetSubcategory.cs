namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetSubcategory
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: Laptops, Monitores, Sillas Giratorias
        public int CategoryId { get; set; }
        public AssetCategory? Category { get; set; }
    }
}
