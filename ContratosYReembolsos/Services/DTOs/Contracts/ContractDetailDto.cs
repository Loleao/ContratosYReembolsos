namespace ContratosYReembolsos.Services.DTOs.Contracts
{
    public class ContractDetailDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;

        // Datos del Afiliado Titular
        public string? AffiliateDni { get; set; }
        public string? AffiliateFullName { get; set; }
        public string? AffiliateCIP { get; set; }

        // Datos del Solicitante y Difunto
        public string SolicitorName { get; set; } = string.Empty;
        public string SolicitorDni { get; set; } = string.Empty;
        public string SolicitorType { get; set; } = string.Empty;
        public string SolicitorCip { get; set; } = string.Empty;
        public string DeceasedName { get; set; } = string.Empty;
        public string DeceasedDni { get; set; } = string.Empty;

        // Logística y Fechas
        public DateTime DeathDate { get; set; }
        public DateTime BurialDate { get; set; }
        public TimeSpan BurialTime { get; set; }
        public string CemeteryName { get; set; } = string.Empty;
        public string UbigeoDetail { get; set; } = string.Empty;
        public string FullLocation { get; set; } = string.Empty;

        // Datos de Velatorio
        public string WakeName { get; set; } = string.Empty;
        public string? CustomWakeAddress { get; set; }

        // Mantenemos las listas simples por compatibilidad con PDFs o listados viejos
        public List<string> Products { get; set; } = new();
        public List<string> Movilities { get; set; } = new();
        public List<string> ExternalServices { get; set; } = new();

        // --- NUEVAS PROPIEDADES DETALLADAS PARA LA VISTA INTERACTIVA ---
        public List<ProductDetailItemDto> DetailedProducts { get; set; } = new();
        public List<ServiceDetailItemDto> DetailedInternalServices { get; set; } = new();
    }

    public class ProductDetailItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Observations { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ServiceDetailItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public string Status { get; set; } = string.Empty;
    }

    // DTO de entrada para la API de confirmación de entregas
    public class ConfirmDeliveryInputDto
    {
        public int Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? Observations { get; set; }
    }
}