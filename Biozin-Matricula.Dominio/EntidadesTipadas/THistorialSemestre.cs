namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class THistorialSemestre
    {
        public string Label { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public decimal? Promedio { get; set; }
        public List<THistorialCurso> Cursos { get; set; } = new();
    }

    public class THistorialCurso
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public decimal? Nota { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
