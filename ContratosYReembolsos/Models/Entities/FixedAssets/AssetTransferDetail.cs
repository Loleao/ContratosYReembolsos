namespace ContratosYReembolsos.Models.Entities.FixedAssets
{
    public class AssetTransferDetail
    {
        public int Id { get; set; }
        public int AssetTransferId { get; set; }

        // Identificamos el activo físico exacto (ID) que se mueve
        public int FixedAssetId { get; set; }
        public virtual FixedAsset? FixedAsset { get; set; }
    }
}
