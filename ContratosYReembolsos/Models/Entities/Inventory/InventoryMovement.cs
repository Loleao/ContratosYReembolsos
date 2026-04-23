using System.ComponentModel.DataAnnotations;
using ContratosYReembolsos.Models.Entities.Branches;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Models.Entities.Inventory
{
    public class InventoryMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public int? FixedAssetId { get; set; }
        public virtual FixedAsset? FixedAsset { get; set; }

        public int? ProductStockId { get; set; }
        public virtual ProductStock? ProductStock { get; set; }

        public decimal Quantity { get; set; }

        public decimal PreviousQuantity { get; set; } // Lo que había antes
        public decimal NewQuantity { get; set; }      // Lo que quedó después

        public Concept Concept { get; set; }

        // Entry (Suma) o Exit (Resta)
        public MovementType MovementType { get; set; }


        public int? TransferId { get; set; }
        public string InternalControlNumber { get; set; }

        public string? ExternalDocumentNumber { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string UserId { get; set; }
    }

    public enum MovementType {
        [Display(Name = "Entrada")]
        Entry,
        [Display(Name = "Salida")]
        Exit 
    }
    public enum Concept {
        [Display(Name = "Compra")]
        Buy,
        [Display(Name = "Transferencia")]
        Transfer,
        [Display(Name = "Ajuste")]
        Adjustment,
        [Display(Name = "Asignación")]
        Assignment,
        [Display(Name = "Baja")]
        Remove 
    }
}
