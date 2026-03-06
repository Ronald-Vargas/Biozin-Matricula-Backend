using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("estudiantes")]
    public class Estudiante
    {
        [Key]
        [Column("id_estudiante")]
        public int IdEstudiante { get; set; }

        [Column("carnet")]
        public long carnet { get; set; }

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

        [Column("email_institucional")]
        public string? EmailInstitucional { get; set; }

        [Column("email_personal")]
        public string? EmailPersonal { get; set; }

        [Column("telefono_movil")]
        public string? TelefonoMovil { get; set; }

        [Column("telefono_emergencia")]
        public string? TelefonoEmergencia { get; set; }

        [Column("nombre_contacto_emergencia")]
        public string? NombreContactoEmergencia { get; set; }

        [Column("provincia")]
        public string? Provincia { get; set; }

        [Column("canton")]
        public string? Canton { get; set; }

        [Column("distrito")]
        public string? Distrito { get; set; }

        [Column("direccion_exacta")]
        public string? DireccionExacta { get; set; }

        [Column("id_carrera")]
        public int? IdCarrera { get; set; }

        [ForeignKey("IdCarrera")]
        public Carrera? Carrera { get; set; }

        [Column("fecha_ingreso")]
        public DateTime? FechaIngreso { get; set; }

        [Column("semestre_actual")]
        public int? SemestreActual { get; set; }

        [Column("estado_estudiante")]
        public string? EstadoEstudiante { get; set; }

        [Column("tipo_beca")]
        public string? TipoBeca { get; set; }

        [Column("condicion_socioeconomica")]
        public string? CondicionSocioeconomica { get; set; }

        [Column("trabaja")]
        public bool? Trabaja { get; set; }

        [Column("colegio_procedencia")]
        public string? ColegioProcedencia { get; set; }

        [Column("tipo_colegio")]
        public string? TipoColegio { get; set; }

        [Column("año_graduacion_colegio")]
        public int? AnioGraduacionColegio { get; set; }

        [Column("discapacidad")]
        public bool? Discapacidad { get; set; }

        [Column("tipo_discapacidad")]
        public string? TipoDiscapacidad { get; set; }

        [Column("necesita_asistencia")]
        public bool? NecesitaAsistencia { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("contraseña")]
        public string Contrasena { get; set; } = string.Empty;
    }
}
