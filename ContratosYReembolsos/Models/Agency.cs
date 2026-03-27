using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ContratosYReembolsos.Models
{
    //[Table("Agencia")]
    public class Agency
    {
        //[Column("codAgen")]
        public string Id { get; set; }
        //[Column("agencia")]
        public string Name { get; set; }
        //[Column("dir_agencia")]
        public string Address { get; set; }
        //[Column("ruc_agencia")]
        public string RUC { get; set; }
        //[Column("tel_agencia")]
        public string Phone { get; set; }

        public string codfilial { get; set; }
        //[ForeignKey("codfilial")]
        //public virtual Subsidiary Filial { get; set; }
    }
}
