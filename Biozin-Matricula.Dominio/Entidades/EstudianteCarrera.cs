using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("estudiante_carreras")]
    public class EstudianteCarrera
    {
        [Key]
        [Column("id_estudiante_carrera")]
        public int IdEstudianteCarrera { get; set; }

        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Column("id_carrera")]
        public int IdCarrera { get; set; }

        [ForeignKey("IdCarrera")]
        public Carrera? Carrera { get; set; }
    }
}
