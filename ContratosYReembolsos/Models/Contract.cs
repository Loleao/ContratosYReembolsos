using System.ComponentModel.DataAnnotations;

public class Contract
{
    public int Id { get; set; }

    public string ContractNumber { get; set; } // Ejemplo: CON-2026-0001
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Status { get; set; } // "Pendiente", "Finalizado", "Cancelado"

    // --- PASO 1: SOLICITANTE ---
    public string SolicitorDni { get; set; }
    public string SolicitorName { get; set; }
    public string SolicitorCip { get; set; }
    public string SolicitorType { get; set; } // "Titular" o "Familiar"

    // --- PASO 2: FALLECIDO ---
    public string DeceasedDni { get; set; }
    public string DeceasedName { get; set; }
    public DateTime DeathDate { get; set; }

    // --- LOGÍSTICA DE SEPULCRO ---
    public DateTime BurialDate { get; set; } // Solo fecha
    public TimeSpan BurialTime { get; set; } // Solo hora (importante para validación de movilidad)

    public string CemeteryId { get; set; } // Ejemplo: CEM120101
    public string BurialType { get; set; } // "Pabellon", "Tumba", "Columbario"
    public string BurialDetail { get; set; } // Texto del mapa: "Pabellón San Juan (Cara 1) - Fila A - Nro 5"
    public int? NicheId { get; set; } // FK opcional al registro real de nicho si lo tienes en DB

    // --- PASO 3: AGENCIA ---
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } // Denormalizado para historial rápido

    // --- RELACIÓN CON SERVICIOS ---
    public virtual ICollection<ContractDetail> Details { get; set; }
}