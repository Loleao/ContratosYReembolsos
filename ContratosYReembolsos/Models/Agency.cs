using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ContratosYReembolsos.Models
{
    public class Agency
    {
        [Key]
        public int Id { get; set; }
        public string RUC { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
