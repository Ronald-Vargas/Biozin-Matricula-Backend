namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TCambioContrasena
    {
        public string Email { get; set; } = string.Empty;
        public string ContrasenaTemporal { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
