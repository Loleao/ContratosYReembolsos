using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models.Entities.Transport
{
    public class PhysicalUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Plate { get; set; } // Placa del vehículo
        public string Brand { get; set; } // Marca
        public bool IsActive { get; set; }
    }
}
