public class ContractUploadViewModel
{
    public SolicitorData Solicitor { get; set; }
    public DeceasedData Deceased { get; set; }
    public AgencyData Agency { get; set; }
    public List<ServiceSelection> Services { get; set; }
    public decimal TotalAmount { get; set; }

    public class SolicitorData
    {
        public string Dni { get; set; }
        public string Name { get; set; }
        public string Cip { get; set; }
        public string Type { get; set; }
    }

    public class DeceasedData
    {
        public string Dni { get; set; }
        public string Name { get; set; }
        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public string BurialTime { get; set; }
        public string Inei { get; set; }
        public string UbigeoFull { get; set; }
        public int? WakeId { get; set; }
        public string WakeName { get; set; }
        public string CemeteryId { get; set; }
        public string CemeteryName { get; set; }
        public string BurialType { get; set; }
        public string BurialDetail { get; set; }
    }

    public class AgencyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class ServiceSelection
    {
        public int ServiceId { get; set; }
        public decimal Price { get; set; }
        public string LogicType { get; set; }
        public ExtraData ExtraData { get; set; }
    }

    public class ExtraData
    {
        public string ScheduledDate { get; set; }
        public string ScheduledTime { get; set; }
        public int? StockItemId { get; set; }
    }
}