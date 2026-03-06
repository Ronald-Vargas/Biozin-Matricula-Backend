using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("carrera_curso")]
    public class CarreraCurso
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_carrera")]
        public int IdCarrera { get; set; }

        [ForeignKey("IdCarrera")]
        public Carrera? Carrera { get; set; }

        [Column("id_curso")]
        public int IdCurso { get; set; }

        [ForeignKey("IdCurso")]
        public Curso? Curso { get; set; }

        [Column("semestre")]
        public int Semestre { get; set; }

    }
}
