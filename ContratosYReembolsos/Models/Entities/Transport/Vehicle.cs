using System.ComponentModel.DataAnnotations;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Models.Entities.Transport
    {
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Plate { get; set; }

        public string Model { get; set; }
        public string Brand { get; set; }
        public int Year { get; set; }

        // RELACIÓN CON FILIAL
        [Required]
        public int BranchId { get; set; } // Usamos int para ser consistentes con tus otras tablas
        public virtual Branch? Branch { get; set; }

        // RELACIÓN CON TIPO DE SERVICIO
        [Required]
        public int VehicleTypeId { get; set; }
        public virtual VehicleType? VehicleType { get; set; }

        public string CurrentStatus { get; set; } = "DISPONIBLE";
        public bool IsActive { get; set; } = true;
    }
}
