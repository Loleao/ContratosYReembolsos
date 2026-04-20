using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; } // Código de referencia genérico
        public ControlType ControlType { get; set; }
        public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.UND;

        public int CategoryId { get; set; }
        public virtual ProductCategory? Category { get; set; }

        public int SubCategoryId { get; set; }
        public virtual ProductSubcategory? SubCategory { get; set; }

        public bool IsAvailableForContract { get; set; } = true;
    }

    public enum ControlType
    {
        Stock,
        Asset 
    }

    public enum UnitOfMeasure
    {
        [Display(Name = "Unidades")]
        UND = 0,

        [Display(Name = "Litros")]
        LTS = 1,

        [Display(Name = "Metros")]
        MTS = 2,

        [Display(Name = "Kilogramos")]
        KGS = 3,

        [Display(Name = "Galones")]
        GLN = 4
    }

}
