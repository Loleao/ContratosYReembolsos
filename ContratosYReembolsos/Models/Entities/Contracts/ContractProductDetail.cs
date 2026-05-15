using ContratosYReembolsos.Models.Entities.FixedAssets;
using ContratosYReembolsos.Models.Entities.Inventory;

namespace ContratosYReembolsos.Models.Entities.Contracts
{
    public class ContractProductDetail
    {
        public int Id { get; set; }

        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }

        // Referencia al producto maestro
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // Cantidad entregada como beneficio
        public decimal Quantity { get; set; }

        public ProductDeliveryStatus Status { get; set; } = ProductDeliveryStatus.Pendiente;

        // Fecha en que el beneficio fue entregado/instalado
        public DateTime DeliveryDate { get; set; } = DateTime.Now;

        // Nota adicional (ej: "Se entrega ataúd modelo estándar por beneficio directo")
        public string? Observations { get; set; }
    }

    public enum ProductDeliveryStatus
    {
        Pendiente = 0,
        Entregado = 1,
        Cancelado = 2
    }
}