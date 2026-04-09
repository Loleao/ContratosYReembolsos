namespace ContratosYReembolsos.Models.ValueObjects
{
    public class BulkEntryRequest
    {
        public int BranchId { get; set; }
        public List<StockEntryItem> Items { get; set; }
    }
}
