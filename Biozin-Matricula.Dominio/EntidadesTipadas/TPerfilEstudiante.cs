namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TPerfilEstudiante
    {
        public int IdEstudiante { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public long Carnet { get; set; }
        public int? IdCarrera { get; set; }
        public string? NombreCarrera { get; set; }
        public int? SemestreActual { get; set; }
        public string? EmailInstitucional { get; set; }
        public bool RequiereCambioContrasena { get; set; }
        public int CreditosAprobados { get; set; }
        public int CreditosMatriculados { get; set; }
        public int CreditosEnCurso { get; set; }
        public int? CreditosTotales { get; set; }
    }
}
