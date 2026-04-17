namespace ContratosYReembolsos.Models
{
    public class ProductTransferDetail
    {
        public int Id { get; set; }
        public int TransferId { get; set; }
        public virtual ProductTransfer? Transfer { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // Si es un activo, debemos saber cuál unidad específica viaja
        public int? FixedAssetId { get; set; }
        public virtual FixedAsset? FixedAsset { get; set; } 

        public int Quantity { get; set; } // Si es activo siempre es 1
    }
}
