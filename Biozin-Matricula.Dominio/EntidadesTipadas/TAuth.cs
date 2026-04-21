using System.ComponentModel.DataAnnotations;

namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TSolicitarRecuperacion
    {
        [Required] public string Email { get; set; } = string.Empty;
    }

    public class TLoginRequest
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Contrasena { get; set; } = string.Empty;
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
        public List<TCarreraResumen>? Carreras { get; set; }
        public int? SemestreActual { get; set; }
        public bool RequiereCambioContrasena { get; set; }
    }
}
