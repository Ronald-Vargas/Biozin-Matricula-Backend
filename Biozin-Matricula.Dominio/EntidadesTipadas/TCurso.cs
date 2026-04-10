namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCurso
    {
        public int IdCurso { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
        public decimal Precio { get; set; }
        public bool TieneLaboratorio { get; set; }
        public decimal PrecioLaboratorio { get; set; }
        public int? idCursoRequisito { get; set; }
        public int HorasDuracion { get; set; }
    }
}
