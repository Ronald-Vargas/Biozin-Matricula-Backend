using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.Entidades
{

    [Table("Aulas")]
    public class Aula
    {


        [Key]
        [Column("idAulas")]
        public int IdAula { get; set; }

        [Column("numeroAula")]
        public string NumeroAula { get; set; } 

        [Column("capacidad")]
        public int Capacidad { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("esLaboratorio")]
        public bool EsLaboratorio { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

    }
}
