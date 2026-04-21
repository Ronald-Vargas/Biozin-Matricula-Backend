using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("ajustes")]
    public class Ajustes
    {
        [Key]
        [Column("idAjuste")]
        public int idAjuste { get; set; }

        [Column("nombreUniversidad")]
        public string nombreUniversidad { get; set; }

        [Column("sitioWeb")]
        public string sitioWeb { get; set; }

        [Column("correoInstitucional")]
        public string correoInstitucional { get; set; }

        [Column("telefono")]
        public string telefono { get; set; }

        [Column("direccion")]
        public string direccion { get; set; }

        [Column("provincia")]
        public string provincia { get; set; }

        [Column("canton")]
        public string canton { get; set; }

        [Column("distrito ")]
        public string distrito { get; set; }

        [Column("montoMatricula")]
        public decimal? montoMatricula { get; set; }

        [Column("montoInfraestructura")]
        public decimal? montoInfraestructura { get; set; }

    }
}
