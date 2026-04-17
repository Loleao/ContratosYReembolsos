public class BulkEntryViewModel
{
    public int BranchId { get; set; }
    public string InternalControlNumber { get; set; }
    public string ExternalDocumentNumber { get; set; }
    public string? Description { get; set; }
    public List<EntryItemViewModel> Items { get; set; }
}

public class EntryItemViewModel
{
    public int ProductId { get; set; }       // Id para Stock
    public int? ProductIdAsset { get; set; } // Id para Activo (si aplica)
    public bool IsAsset { get; set; }
    public int Quantity { get; set; }
    public string? SerialNumber { get; set; }
}