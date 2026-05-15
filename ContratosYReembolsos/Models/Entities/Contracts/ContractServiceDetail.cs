using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class ContractServiceDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }

        public int FuneralServiceId { get; set; }
        public virtual FuneralService FuneralService { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public string? Observations { get; set; }

        public ServiceExecutionStatus Status { get; set; } = ServiceExecutionStatus.Pendiente;
    }

    public enum ServiceExecutionStatus
    {
        Pendiente = 0,
        EnProgreso = 1,
        Ejecutado = 2,
        Cancelado = 3
    }
}
