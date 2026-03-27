using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Models.External
{
    [Table("Beneficiarios")]
    [PrimaryKey(nameof(idfaf), nameof(codBenef))]
    public class Beneficiary
    {
        [Column("idfaf")]
        public string? idfaf { get; set; }
        [Column("codBenef")]
        public string? codBenef { get; set; }
        [Column("nombre")]
        public string? Name { get; set; }
        [Column("fecNac")]
        public DateTime? BirthDate { get; set; }
        [Column("codParent")]
        public string? codParent { get; set; }
    }
}
