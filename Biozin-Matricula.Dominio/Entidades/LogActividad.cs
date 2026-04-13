using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("log_actividad")]
    public class LogActividad
    {
        [Key]
        [Column("id_log")]
        public int IdLog { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("icono")]
        public string Icono { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
