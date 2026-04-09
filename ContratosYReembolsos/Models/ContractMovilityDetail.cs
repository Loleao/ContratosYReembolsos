using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class ContractMovilityDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        // Usamos tu clase estática TipoMovilidad (RECOJO, CARROZA, etc.)
        [Required]
        public string ServiceType { get; set; }

        public bool IsDispatched { get; set; } = false; // ¿Ya se asignó vehículo/chofer?

        public DateTime? ScheduledDate { get; set; }

        // --- ASIGNACIÓN (Estos campos los llena el módulo de Movilidad luego) ---
        public int? VehicleId { get; set; } // De tu tabla de Vehículos
        public int? DriverId { get; set; }  // De tu tabla de Conductores
    }
}