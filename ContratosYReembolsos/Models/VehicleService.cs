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

        [Required]
        public string ServiceType { get; set; }

        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; } // Hora de salida

        public DateTime? ReturnTime { get; set; } // Nulo hasta que el carro regrese

        // Estados del viaje: EN_RUTA, FINALIZADO, CANCELADO
        public string TripStatus { get; set; } = "EN_RUTA";

        public string Observations { get; set; } // Ej: "Regresó con poco combustible"

        // Propiedad calculada para saber cuánto duró el servicio
        public TimeSpan? Duration => ReturnTime.HasValue ? ReturnTime - DepartureTime : null;
    }
}
