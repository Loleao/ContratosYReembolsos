using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
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