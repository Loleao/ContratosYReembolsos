namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetHistory
    {
        public int Id { get; set; }
        public int FixedAssetId { get; set; }
        public FixedAsset? FixedAsset { get; set; }

        public int? FromBranchId { get; set; }
        public int? ToBranchId { get; set; }

        public string ResponsibleUserId { get; set; } // DNI o Nombre de quien recibe el activo
        public string Action { get; set; } // "Alta", "Asignación", "Mantenimiento", "Transferencia"

        public DateTime Date { get; set; } = DateTime.Now;
        public string Note { get; set; }
    }
}
