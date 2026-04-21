namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TMatricularPeriodo
    {
        public int IdPeriodo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaMatriculaFin { get; set; }
        public List<TOfertaDisponible> Ofertas { get; set; } = new();
        public decimal MontoMatricula { get; set; }
        public decimal MontoInfraestructura { get; set; }
    }
}
