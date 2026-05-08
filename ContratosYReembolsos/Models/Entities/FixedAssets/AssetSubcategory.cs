namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetSubcategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public AssetCategory? Category { get; set; }
    }
}
