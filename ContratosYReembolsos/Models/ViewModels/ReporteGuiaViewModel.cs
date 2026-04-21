namespace ContratosYReembolsos.Models.ViewModels
{
    public class ReporteGuiaViewModel
    {
        public string NumeroGuia { get; set; }
        public string TipoOperacion { get; set; }
        public DateTime Fecha { get; set; }
        public string SedeOrigen { get; set; }
        public string SedeDestino { get; set; }
        public string UsuarioResponsable { get; set; }
        public string DocumentoExterno { get; set; }
        public string EstadoMensaje { get; set; } // Agregado para el banner de estado
        public List<DetalleGuiaItem> Items { get; set; }
    }

    public class DetalleGuiaItem
    {
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public string Tipo { get; set; } // ENTRADA o SALIDA
        public string Sede { get; set; }

        // ESTA ES LA LÍNEA QUE TE FALTA:
        public string CodigoPatrimonial { get; set; }
    }
}