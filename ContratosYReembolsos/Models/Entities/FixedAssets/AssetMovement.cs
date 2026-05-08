using System.ComponentModel.DataAnnotations;
using ContratosYReembolsos.Models.Entities.Branches;

namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetMovement
    {
        public int Id { get; set; }

        // El activo específico que se mueve
        public int FixedAssetId { get; set; }
        public virtual FixedAsset? FixedAsset { get; set; }

        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        // Conceptos y Tipos (Reutilizamos tus Enums o creamos unos específicos)
        public AssetMovementType MovementType { get; set; }
        public AssetConcept Concept { get; set; }

        public string InternalControlNumber { get; set; } // Ej: NI-2026-00001
        public string? ExternalDocumentNumber { get; set; } // Factura/Guía
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string UserId { get; set; }
    }

    public enum AssetMovementType
    {
        [Display(Name = "Entrada")] Entry,
        [Display(Name = "Salida")] Exit
    }

    public enum AssetConcept
    {
        [Display(Name = "Compra / Alta")] Buy,
        [Display(Name = "Transferencia")] Transfer,
        [Display(Name = "Baja Patrimonial")] Retirement,
        [Display(Name = "Ajuste de Inventario")] Adjustment
    }
}