namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCarrera
    {
        public int IdCarrera { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int Duracion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
