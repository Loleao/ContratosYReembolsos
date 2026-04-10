    using System.ComponentModel.DataAnnotations;

    namespace ContratosYReembolsos.Models
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
