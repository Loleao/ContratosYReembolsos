using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.External
{
    [Table("Afiliados")]
    public class Affiliate
    {
        [Key]
        [Column("idfaf")]
        public string? Id { get; set; }
        [Column("nombre")]
        public string? Name { get; set; }
        [Column("dni")]
        public string? DNI { get; set; }
        [Column("telefono")]
        public string? Phone { get; set; }
        [Column("cip")]
        public string? CIP { get; set; }
        [Column("fecNac")]
        public string? BirthDate { get; set; }
    }
}
