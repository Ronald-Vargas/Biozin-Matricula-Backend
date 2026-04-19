using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("pago_matriculas")]
    public class PagoMatricula
    {
        [Key]
        [Column("id_pago_matricula")]
        public int IdPagoMatricula { get; set; }

        [Column("id_pago")]
        public int IdPago { get; set; }

        [ForeignKey("IdPago")]
        public Pago? Pago { get; set; }

        [Column("id_matricula")]
        public int IdMatricula { get; set; }

        [ForeignKey("IdMatricula")]
        public Matricula? Matricula { get; set; }
    }
}
