namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCursoMalla
    {
        public int IdCurso { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public int Semestre { get; set; }
        public bool EsVirtual { get; set; }
        public int? IdCursoRequisito { get; set; }
        public string? NombreRequisito { get; set; }
        public string Estado { get; set; } = "pendiente"; // aprobado | en_curso | disponible | pendiente
        public decimal? Nota { get; set; }
    }

    public class TSemestreMalla
    {
        public int Numero { get; set; }
        public List<TCursoMalla> Cursos { get; set; } = new();
    }

    public class TMallaCurricular
    {
        public string NombreCarrera { get; set; } = string.Empty;
        public int TotalCreditos { get; set; }
        public int CreditosAprobados { get; set; }
        public int CreditosEnCurso { get; set; }
        public List<TSemestreMalla> Semestres { get; set; } = new();
    }
}
