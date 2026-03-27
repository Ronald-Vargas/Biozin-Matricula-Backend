namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TPagoEstudiante
    {
        public int IdPago { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
    }
}
