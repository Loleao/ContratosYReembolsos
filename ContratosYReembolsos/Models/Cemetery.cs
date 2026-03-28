using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContratosYReembolsos.Models
{
    public class Cemetery
    {
        [Key]
        public string Id { get; set; }
        public string RUC { get; set; }
        public string Name { get; set; }
        public string UbigeoId { get; set; }
    }
}
