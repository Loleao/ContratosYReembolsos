using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; } // Ej: "Ataúdes"
        public virtual ICollection<ProductSubcategory>? SubCategories { get; set; }
    }
}
