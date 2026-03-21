namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TOfertaAcademica
    {
        public int IdOferta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdPeriodo { get; set; }
        public int IdCurso { get; set; }
        public int IdProfesor { get; set; }
        public int IdAula { get; set; }
        public int CupoMaximo { get; set; }
        public int Matriculados { get; set; }
        public decimal Precio { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public List<TDiaHorario> DiasHorarios { get; set; } = new List<TDiaHorario>();
    }
}
