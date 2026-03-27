using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.External
{
    [Table("ubigeo")]
    public class Ubigeo
    {
        [Key]
        [Column("codigo")]
        public string INEI { get; set; }
        [Column("departamento")]
        public string? Region { get; set; }
        [Column("provincia")]
        public string? Province { get; set; }
        [Column("distrito")]
        public string? District { get; set; }
    }
}
