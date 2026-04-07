namespace ContratosYReembolsos.Models.ValueObjects
{
    public class RegionCode
    {
        private static readonly Dictionary<string, string> RegionMap = new()
        {
            {"AMAZONAS", "AMA"}, 
            {"ÁNCASH", "ANC"}, 
            {"APURÍMAC", "APU"},
            {"AREQUIPA", "ARE"}, 
            {"AYACUCHO", "AYA"}, 
            {"CAJAMARCA", "CAJ"},
            {"CALLAO", "CAL"}, 
            {"CUSCO", "CUS"}, 
            {"HUANCAVELICA", "HUV"},
            {"HUÁNUCO", "HUA"}, 
            {"ICA", "ICA"}, 
            {"JUNÍN", "JUN"},
            {"LA LIBERTAD", "LAL"}, 
            {"LAMBAYEQUE", "LAM"}, 
            {"LIMA", "LIM"},
            {"LORETO", "LOR"}, 
            {"MADRE DE DIOS", "MDD"}, 
            {"MOQUEGUA", "MOQ"},
            {"PASCO", "PAS"}, 
            {"PIURA", "PIU"}, 
            {"PUNO", "PUN"},
            {"SAN MARTÍN", "SAM"}, 
            {"TACNA", "TAC"}, 
            {"TUMBES", "TUM"},
            {"UCAYALI", "UCA"}
        };

        public string Prefix { get; }

        public RegionCode(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("La región no puede estar vacía.");

            string key = regionName.ToUpper().Trim();
            Prefix = RegionMap.ContainsKey(key) ? RegionMap[key] : "GEN";
        }

        public string GenerateFullCode(int nextNumber)
        {
            return $"{Prefix}{nextNumber}";
        }

        // Sobrescribimos para que al imprimir el objeto nos dé el prefijo
        public override string ToString() => Prefix;
    }
}