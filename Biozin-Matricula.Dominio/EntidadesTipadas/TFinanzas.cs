namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TResumenFinanzas
    {
        public int? IdPeriodo { get; set; }
        public string NombrePeriodo { get; set; } = string.Empty;
        public decimal TotalRecaudado { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TotalVencido { get; set; }
        public int CantidadPagados { get; set; }
        public int CantidadPendientes { get; set; }
        public int CantidadVencidos { get; set; }
        public int TotalMatriculas { get; set; }
    }

    public class TDetallePago
    {
        public int IdPago { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public string CarnetEstudiante { get; set; } = string.Empty;
        public string NombrePeriodo { get; set; } = string.Empty;
    }
}
