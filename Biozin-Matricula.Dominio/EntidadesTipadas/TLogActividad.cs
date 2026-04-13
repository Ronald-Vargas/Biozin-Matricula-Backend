namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TLogActividad
    {
        public int IdLog { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
