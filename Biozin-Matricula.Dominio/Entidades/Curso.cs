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

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("tiene_laboratorio")]
        public bool TieneLaboratorio { get; set; }

        [Column("precio_laboratorio")]
        public decimal PrecioLaboratorio { get; set; }

        [Column ("idCursoRequisito")]
        public int? idCursoRequisito { get; set; }

        [Column("horas_duracion")]
        public int HorasDuracion { get; set; }

        [Column("es_virtual")]
        public bool EsVirtual { get; set; } = false;

    }
}
