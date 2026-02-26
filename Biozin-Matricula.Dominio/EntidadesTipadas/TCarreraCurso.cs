namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCarreraCurso
    {
        public int Id { get; set; }
        public int IdCarrera { get; set; }
        public int IdCurso { get; set; }
        public int Semestre { get; set; }
        public bool EsObligatorio { get; set; } = true;
    }
}
