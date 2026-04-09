namespace ContratosYReembolsos.Models.ValueObjects
{
    public static class TipoMovilidad
    {
        public const string Recojo = "RECOJO";
        public const string Carroza = "CARROZA";
        public const string Flores = "FLORES";
        public const string Custer = "CUSTER";

        public static List<string> ObtenerTodos() => new()
        {
            Recojo, Carroza, Flores, Custer
        };

        public static string GetIcon(string tipo) => tipo switch
        {
            Recojo => "fa-hand-holding-medical",
            Carroza => "fa-truck-medical",
            Flores => "fa-leaf",
            Custer => "fa-bus",
            _ => "fa-truck"
        };
    }
}
