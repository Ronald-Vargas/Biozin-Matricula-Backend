namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCredencialesEstudiante
    {
        public int IdEstudiante { get; set; }
        public long Carnet { get; set; }
        public string EmailInstitucional { get; set; } = string.Empty;
        public string ContrasenaGenerada { get; set; } = string.Empty;
    }
}
