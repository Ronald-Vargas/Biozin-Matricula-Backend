namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }

    public class TAuthRespuesta
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Campos exclusivos de estudiante (null para administradores)
        public long? Carnet { get; set; }
        public int? IdCarrera { get; set; }
        public string? NombreCarrera { get; set; }
        public int? SemestreActual { get; set; }
    }
}
