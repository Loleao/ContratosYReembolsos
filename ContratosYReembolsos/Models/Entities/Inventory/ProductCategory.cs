using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.Entities.Inventory
{
    public class ProductCategory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } // Ej: "Ataúdes"
        public bool ShowInContracts { get; set; } = true;
        public virtual ICollection<ProductSubcategory>? SubCategories { get; set; }
    }
}
