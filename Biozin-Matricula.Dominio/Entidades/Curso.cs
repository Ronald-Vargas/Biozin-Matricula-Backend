using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("cursos")]
    public class Curso
    {
        [Key]
        [Column("id_curso")]
        public int IdCurso { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("creditos")]
        public int Creditos { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;
    }
}
