namespace ContratosYReembolsos.Models.Entities.Inventory
{
    public class FixedAsset
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int BranchId { get; set; }

        // Identidad única solicitada
        public string PatrimonialCode { get; set; }
        public string SerialNumber { get; set; } // Placa o serie de fábrica
        public string Status { get; set; } // "Disponible", "En Uso", "Mantenimiento"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum AssetStatus
    {
        Available,     // Operativo en sede
        InUse,         // Asignado a un trabajador/servicio
        Maintenance,   // En taller
        Retired        // Dado de baja (obsoleto/malogrado)
    }
}
