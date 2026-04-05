namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TLoginRespuesta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool RequiereCambioContrasena { get; set; }
    }
}
