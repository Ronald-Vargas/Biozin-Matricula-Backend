using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("matriculas")]
    public class Matricula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_matricula")]
        public int IdMatricula { get; set; }

        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [ForeignKey("IdEstudiante")]
        public Estudiante? Estudiante { get; set; }

        [Column("id_oferta")]
        public int IdOferta { get; set; }

        [ForeignKey("IdOferta")]
        public OfertaAcademica? OfertaAcademica { get; set; }

        [Column("fecha_matricula")]
        public DateTime FechaMatricula { get; set; } = DateTime.UtcNow;

        [Column("nota")]
        public decimal? Nota { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "en_curso";
    }
}
