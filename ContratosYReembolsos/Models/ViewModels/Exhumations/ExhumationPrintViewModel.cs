namespace ContratosYReembolsos.Models.ViewModels.Exhumations
{
    public class ExhumationPrintViewModel
    {
        public string Title { get; set; } = "CONSTANCIA DE EXHUMACIÓN";
        public string Folio { get; set; }
        public string FechaEmision { get; set; }
        public string NombreFallecido { get; set; }
        public string Dni { get; set; }
        public string UbicacionOrigen { get; set; }
        public string UbicacionDestino { get; set; }
        public string TipoTramite { get; set; }
        public string MontoTotal { get; set; } // S/ 1,000.00
        public string Notas { get; set; }
    }
}