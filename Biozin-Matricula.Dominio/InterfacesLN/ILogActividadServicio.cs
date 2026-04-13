using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface ILogActividadServicio
    {
        void Registrar(string tipo, string descripcion, string icono);
        Respuesta<List<TLogActividad>> ObtenerRecientes(int cantidad = 20);
    }
}
