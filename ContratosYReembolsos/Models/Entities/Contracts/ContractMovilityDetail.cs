using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContratosYReembolsos.Models.Entities.Transport;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class ContractMovilityDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }

        public int VehicleTypeId { get; set; }
        public virtual VehicleType? VehicleType { get; set; }

        public string Status { get; set; } = "PENDIENTE";
    }
}