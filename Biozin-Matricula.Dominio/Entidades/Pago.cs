using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_matricula")]
        public int IdMatricula { get; set; }

        [ForeignKey("IdMatricula")]
        public Matricula? Matricula { get; set; }

        [Column("concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Column("monto")]
        public decimal Monto { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";
    }
}
