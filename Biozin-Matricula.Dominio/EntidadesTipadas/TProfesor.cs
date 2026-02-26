namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TProfesor
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Nacionalidad { get; set; }
        public string? EmailPersonal { get; set; }
        public string? Telefono { get; set; }
        public string? Titulo { get; set; }
        public string? Especialidad { get; set; }
        public string? CursosAsignados { get; set; }
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Distrito { get; set; }
        public string? Direccion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? EmailInstitucional { get; set; }
        public string Contrasena { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public bool Estado { get; set; } = true;
    }
}
