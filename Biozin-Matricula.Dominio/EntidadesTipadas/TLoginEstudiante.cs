namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TLoginEstudiante
    {
        public string EmailInstitucional { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }

    public class TLoginRespuesta
    {
        public string Token { get; set; } = string.Empty;
        public TPerfilEstudiante Perfil { get; set; } = new();
    }
}
