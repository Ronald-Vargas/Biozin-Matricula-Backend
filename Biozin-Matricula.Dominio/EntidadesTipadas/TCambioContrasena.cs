using System.ComponentModel.DataAnnotations;

namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCambioContrasena
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string ContrasenaTemporal { get; set; } = string.Empty;
        [Required] public string NuevaContrasena { get; set; } = string.Empty;
    }
}
