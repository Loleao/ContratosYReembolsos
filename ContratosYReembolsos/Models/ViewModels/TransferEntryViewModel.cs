namespace ContratosYReembolsos.Models.ViewModels
{
    public class TransferEntryViewModel
    {
        public string InternalControlNumber { get; set; }
        public int OriginBranchId { get; set; }
        public int TargetBranchId { get; set; }
        public string? Observation { get; set; }

        public List<TransferItemEntryViewModel> Items { get; set; } = new();
    }

    public class TransferItemEntryViewModel
    {
        public int ProductId { get; set; }        // Para stock consumible
        public int? FixedAssetId { get; set; }    // Para activos específicos
        public bool IsAsset { get; set; }
        public int Quantity { get; set; }
    }
}
