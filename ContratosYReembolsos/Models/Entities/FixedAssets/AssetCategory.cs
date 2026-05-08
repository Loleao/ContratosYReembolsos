namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ej: Equipos de Cómputo, Vehículos, Mobiliario
        public string? Description { get; set; }

        public virtual ICollection<AssetSubcategory>? Subcategories { get; set; }
    }
}
