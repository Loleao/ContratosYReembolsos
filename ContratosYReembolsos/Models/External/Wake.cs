using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.External
{
    [Table("velatorio")]
    public class Wake
    {
        [Column("codVelatorio")]
        public string? Id { get; set; }
        [Column("nombre")]
        public string? Name { get; set; }
    }
}
