using ContratosYReembolsos.Models;

public class ContractDetail
{
    public int Id { get; set; }
    public int ContractId { get; set; }

    public int ServiceId { get; set; }
    public virtual Service Service { get; set; }

    public DateTime? ScheduledDate { get; set; }
    public TimeSpan? ScheduledTime { get; set; }

    public int? StockItemId { get; set; }
    public virtual StockItem StockItem { get; set; }

    public string Observations { get; set; }
}