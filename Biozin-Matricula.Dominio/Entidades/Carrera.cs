using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("carreras")]
    public class Carrera
    {
        [Key]
        [Column("id_carrera")]
        public int IdCarrera { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("duracion")]
        public int Duracion { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;
    }
}
