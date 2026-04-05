namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TEstudiante {
        public int IdEstudiante { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EmailInstitucional { get; set; }
        public string? EmailPersonal { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? TelefonoEmergencia { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Distrito { get; set; }
        public string? DireccionExacta { get; set; }
        public int? IdCarrera { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public int? SemestreActual { get; set; }
        public bool? EstadoEstudiante { get; set; }
        public string? TipoBeca { get; set; }
        public string? CondicionSocioeconomica { get; set; }
        public bool? Trabaja { get; set; }
        public string? ColegioProcedencia { get; set; }
        public string? TipoColegio { get; set; }
        public int? AnioGraduacionColegio { get; set; }
        public bool? Discapacidad { get; set; }
        public string? TipoDiscapacidad { get; set; }
        public bool? NecesitaAsistencia { get; set; }
        public string? Observaciones { get; set; }
        public string Contrasena { get; set; } = string.Empty;

        public long carnet { get; set; }

        public bool RequiereCambioContrasena { get; set; } = true;
    }
}
