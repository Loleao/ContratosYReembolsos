using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContratosYReembolsos.Models.External
{
    [Table("Filial")]
    public class Subsidiary
    {
        [Column("codfilial")]
        public string Id { get; set; }
        [Column("descripcion")]
        public string? Name { get; set; }
        [Column("direccion")]
        public string? Address { get; set; }
        [Column("telefono")]
        public string? Phone { get; set; }
        [Column("codUbigeo")]
        public string? UbigeoId { get; set; }
    }
}
