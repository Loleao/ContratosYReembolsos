using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.Entities.Agencies
{
    public class Ubigeo
    {
        [Key]
        [StringLength(6)]
        public string Id { get; set; } // Ej: 080101

        public string Region { get; set; }
        public string Province { get; set; }
        public string District { get; set; }

        [StringLength(3)]
        public string Abbreviation { get; set; }
    }
}
