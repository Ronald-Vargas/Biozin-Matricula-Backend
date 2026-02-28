namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCredencialesProfesor
    {
        public int IdProfesor { get; set; }
        public string EmailInstitucional { get; set; } = string.Empty;
        public string ContrasenaGenerada { get; set; } = string.Empty;
    }
}
