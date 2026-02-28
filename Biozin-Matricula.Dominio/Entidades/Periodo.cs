using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("periodos")]
    public class Periodo
    {
        [Key]
        [Column("id_periodo")]
        public int IdPeriodo { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("fecha_matricula_ini")]
        public DateTime FechaMatriculaInicio { get; set; }

        [Column("fecha_matricula_fin")]
        public DateTime FechaMatriculaFin { get; set; }

        [Column("estado_matricula")]
        public bool EstadoMatricula { get; set; } = false;

    }
}
