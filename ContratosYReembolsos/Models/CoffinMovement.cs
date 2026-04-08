using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models
{
    public class CoffinMovement
    {
        public int Id { get; set; }

        public int CoffinVariantId { get; set; }
        public CoffinVariant CoffinVariant { get; set; }

        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }

        public int Quantity { get; set; }

        // Sugerencia de Tipos: "INGRESO_PROVEEDOR", "TRANSFERENCIA_IN", "TRANSFERENCIA_OUT", "SALIDA_CONTRATO"
        public string Type { get; set; }

        public string? Reference { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Observations { get; set; }

        // ¿Quién hizo el movimiento?
        public string? RegisteredBy { get; set; }

        // Stock que quedó en la filial tras este movimiento
        public int BalanceAfter { get; set; }
    }
}