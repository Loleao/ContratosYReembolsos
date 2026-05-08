using ContratosYReembolsos.Models.Entities.Branches; // Para la relación con Filial

namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class FixedAsset
    {
        public int Id { get; set; }

        // Relación con el Catálogo (El "Qué es")
        public int AssetCatalogId { get; set; }
        public AssetCatalog AssetCatalog { get; set; }

        // Ubicación
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        // Identificadores únicos
        public string PatrimonialCode { get; set; } // Generado por sistema (Ej: FON-2026-0001)
        public string SerialNumber { get; set; } // El que viene de fábrica

        // Estados
        public AssetStatus Status { get; set; }
        public AssetCondition Condition { get; set; } // Nuevo campo: ¿En qué estado físico está?

        // Auditoría y Registro
        public string? Observation { get; set; }
        public DateTime PurchaseDate { get; set; } // Útil para depreciación futura
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string RegisteredByUserId { get; set; } // Quién hizo el alta
    }

    public enum AssetStatus
    {
        Available,    // En oficina listo para usar
        InUse,        // Asignado a una persona
        Maintenance,  // En reparación
        Retired,      // Baja definitiva
        Transferred   // En proceso de mudanza entre sedes
    }

    public enum AssetCondition
    {
        New,        // Nuevo (en caja)
        Excellent,  // Como nuevo
        Good,       // Uso normal, operativo
        Fair,       // Desgastado pero operativo
        Poor,       // Mal estado
        Broken      // No funciona
    }
}