namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TMatricularBulkSolicitud
    {
        public List<int> IdsOferta { get; set; } = new();
        public bool Financiar { get; set; }
        public string MetodoPago { get; set; } = "tarjeta";
    }
}
