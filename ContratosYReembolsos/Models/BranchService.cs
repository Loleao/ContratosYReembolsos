namespace ContratosYReembolsos.Models
{
    public class BranchMovilityService
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }

        public int BranchId { get; set; } // A qué filial pertenece esta oferta
        public virtual Branch Branch { get; set; }

        public decimal Price { get; set; } // Precio específico en esa filial (puede variar)
        public bool IsActive { get; set; }
    }
}
