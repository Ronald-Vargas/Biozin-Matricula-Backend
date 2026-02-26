namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TPeriodo
    {
        public int IdPeriodo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaMatriculaIni { get; set; }
        public DateTime FechaMatriculaFin { get; set; }
        public bool EstadoMatricula { get; set; } = false;
        public DateTime FechaCreacion { get; set; }
    }
}
