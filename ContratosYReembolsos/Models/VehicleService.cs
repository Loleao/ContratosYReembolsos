using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class VehicleService
    {
        public int Id { get; set; }

        // Relaciones
        [Required]
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; }

        [Required]
        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }

        public int? ContractMovilityDetailId { get; set; }
        public virtual ContractMovilityDetail? ContractMovilityDetail { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }
        public DateTime? ReturnTime { get; set; }

        // Estados del viaje: EN_RUTA, FINALIZADO, CANCELADO
        public string TripStatus { get; set; } = "EN_RUTA";

        public string? Observations { get; set; }

        public TimeSpan? Duration => ReturnTime.HasValue ? ReturnTime - DepartureTime : null;
    }
}
