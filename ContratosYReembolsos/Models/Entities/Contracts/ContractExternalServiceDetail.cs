using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class ContractExternalServiceDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        public int FuneralServiceId { get; set; }
        public virtual FuneralService FuneralService { get; set; }

        public int Quantity { get; set; } = 1;

        // En convenios a veces el precio es 0 para el usuario, 
        // pero podemos guardar el valor de referencia.
        [Column(TypeName = "decimal(18,2)")]
        public decimal ReferencePrice { get; set; }
    }
}
