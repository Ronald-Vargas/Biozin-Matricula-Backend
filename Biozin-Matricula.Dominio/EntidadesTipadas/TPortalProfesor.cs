namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TPerfilProfesor
    {
        public int IdProfesor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string EmailInstitucional { get; set; } = string.Empty;
        public string? Titulo { get; set; }
        public string? Especialidad { get; set; }
        public int CursosAsignados { get; set; }
    }

    public class TOfertaProfesor
    {
        public int IdOferta { get; set; }
        public string CodigoCurso { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;
        public string? NombreAula { get; set; }
        public string? Horario { get; set; }
        public int CupoMaximo { get; set; }
        public int Matriculados { get; set; }
        public bool Estado { get; set; }
    }

    public class TEstudianteEnCurso
    {
        public int IdMatricula { get; set; }
        public int IdEstudiante { get; set; }
        public long Carnet { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string EmailInstitucional { get; set; } = string.Empty;
        public decimal? Nota { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class TAsignarNota
    {
        public int IdMatricula { get; set; }
        public decimal Nota { get; set; }
    }
}
