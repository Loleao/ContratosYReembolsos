namespace ContratosYReembolsos.Models.ValueObjects
{
    public class BulkTransferRequest
    {
        public int TargetBranchId { get; set; }
        public string Reference { get; set; }
        public List<TransferItem> Items { get; set; }
    }
}
