using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    [Table("CementerioProvincias")]
    public class Cemetery
    {
        //[Column("codCemente")]
        public string Id { get; set; }
        //[Column("nombre")]
        public string Name { get; set; }
        //[Column("ruc_cemente")]
        public string RUC { get; set; }
        //[Column("codUbigeo")]
        public string UbigeoId { get; set; }
    }
}
