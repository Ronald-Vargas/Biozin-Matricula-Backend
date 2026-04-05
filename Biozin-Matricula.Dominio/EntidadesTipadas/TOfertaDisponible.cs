namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TOfertaDisponible
    {
        public int IdOferta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public string? Aula { get; set; }
        public List<TDiaHorario>? Horario { get; set; }
        public int Creditos { get; set; }
        public int CupoMaximo { get; set; }
        public int Matriculados { get; set; }
        public decimal Precio { get; set; }
    }
}
