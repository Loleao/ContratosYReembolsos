using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class ServiceContract
    {
        [Key]
        [MaxLength(20)]
        public string id { get; set; }

        [MaxLength(4)]
        public string año_contrato { get; set; }

        [MaxLength(4)]
        public string num_contrato { get; set; }

        [MaxLength(1)]
        public string tipoSoli { get; set; }

        [MaxLength(15)]
        public string idfaf { get; set; }

        [MaxLength(150)]
        public string Solicitante { get; set; }

        [MaxLength(150)]
        public string domicilio_soli { get; set; }

        [MaxLength(15)]
        public string dni_soli { get; set; }

        [MaxLength(20)]
        public string tel_soli { get; set; }

        [MaxLength(20)]
        public string cip_titular { get; set; }

        [MaxLength(1)]
        public string tipoDifun { get; set; }

        [MaxLength(10)]
        public string codBenef { get; set; }

        [MaxLength(10)]
        public string codAgen { get; set; }

        [MaxLength(360)]
        public string Difunto { get; set; }

        [MaxLength(15)]
        public string dni_difu { get; set; }

        public DateTime? fecha_nacimiento { get; set; }

        [MaxLength(120)]
        public string velatorio { get; set; }
        [MaxLength(120)]
        public string fallecimiento_lugar { get; set; }

        [MaxLength(10)]
        public string hor_sepelio { get; set; }

        [MaxLength(4)]
        public string codCemente { get; set; }

        [MaxLength(6)]
        public string codfilial { get; set; }

        public DateTime? fec_contrato { get; set; }

        public DateTime? fec_falleci { get; set; }

        public DateTime? fec_sepelio { get; set; }

        [MaxLength(15)]
        public string CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }

        [MaxLength(15)]
        public string ModificadoPor { get; set; }

        public DateTime? FechaModificacion { get; set; }
        //public virtual Affiliate Afiliado { get; set; }
        //public virtual Beneficiary Beneficiario { get; set; }
        //public virtual Subsidiary Filial { get; set; }
        //public virtual Agency Agencia { get; set; }
        //public virtual Cemetery CementarioProvincias { get; set; }
    }
}
