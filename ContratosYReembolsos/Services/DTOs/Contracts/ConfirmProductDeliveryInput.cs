namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class ConfirmProductDeliveryInput
    {
        public int Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Observations { get; set; }
    }
}
