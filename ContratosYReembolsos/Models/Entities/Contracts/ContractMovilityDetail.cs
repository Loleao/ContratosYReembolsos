using ContratosYReembolsos.Models.Entities.Transport;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class ContractMovilityDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }

        // Tipo de servicio (Carroza, Bus de acompañantes, etc.)
        public int VehicleTypeId { get; set; }
        public virtual VehicleType? VehicleType { get; set; }

        // Control del servicio
        public string Status { get; set; } = "PENDIENTE";

        // Datos logísticos específicos del servicio
        public string? OriginLocation { get; set; }
        public string? DestinationLocation { get; set; }

        // Vinculación opcional al vehículo real una vez asignado
        public int? VehicleId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}