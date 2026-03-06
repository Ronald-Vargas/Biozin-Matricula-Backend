using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("profesores")]
    public class Profesor
    {
        [Key]
        [Column("id")]
        public int IdProfesor { get; set; }

        [Column("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("apellido_paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Column("apellido_materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Column("fecha_nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("genero")]
        public string? Genero { get; set; }

        [Column("nacionalidad")]
        public string? Nacionalidad { get; set; }

        [Column("email_personal")]
        public string? EmailPersonal { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("titulo")]
        public string? Titulo { get; set; }

        [Column("especialidad")]
        public string? Especialidad { get; set; }

        [Column("cursos_asignados")]
        public int? CursosAsignados { get; set; }

        [Column("provincia")]
        public string? Provincia { get; set; }

        [Column("canton")]
        public string? Canton { get; set; }

        [Column("distrito")]
        public string? Distrito { get; set; }

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("email_institucional")]
        public string? EmailInstitucional { get; set; }

        [Column("contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Column("fecha_ingreso")]
        public DateTime? FechaIngreso { get; set; }

        [Column("estado")]
        public string Estado { get; set; } 
    }
}
