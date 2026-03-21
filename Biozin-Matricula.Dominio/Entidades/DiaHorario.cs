using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("dia_horario")]
    public class DiaHorario
    {
        [Key]
        [Column("id_dia_horario")]
        public int IdDiaHorario { get; set; }

        [Column("id_oferta")]
        public int IdOferta { get; set; }

        [ForeignKey("IdOferta")]
        public OfertaAcademica? OfertaAcademica { get; set; }

        [Column("dia")]
        public string Dia { get; set; } = string.Empty;

        [Column("hora_inicio")]
        public string HoraInicio { get; set; } = string.Empty;

        [Column("hora_fin")]
        public string HoraFin { get; set; } = string.Empty;
    }
}
