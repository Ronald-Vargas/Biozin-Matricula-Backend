using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("oferta_academica")]
    public class OfertaAcademica
    {
        [Key]
        [Column("id_oferta")]
        public int IdOferta { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("id_periodo")]
        public int IdPeriodo { get; set; }

        [ForeignKey("IdPeriodo")]
        public Periodo? Periodo { get; set; }

        [Column("id_curso")]
        public int IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso? Curso { get; set; }

        [Column("id_profesor")]
        public int IdProfesor { get; set; }

        [ForeignKey("IdProfesor")]
        public Profesor? Profesor { get; set; }

        [Column("id_aula")]
        public int IdAula { get; set; }

        [ForeignKey("IdAula")]
        public Aula? Aula { get; set; }

        [Column("cupo_maximo")]
        public int CupoMaximo { get; set; }

        [Column("matriculados")]
        public int Matriculados { get; set; } = 0;

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<DiaHorario> DiasHorarios { get; set; } = new List<DiaHorario>();
    }
}
