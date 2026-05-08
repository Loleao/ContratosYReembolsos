using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class FuneralService
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}