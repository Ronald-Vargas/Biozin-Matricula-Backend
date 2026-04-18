using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IFinanzasLN
    {
        Respuesta<TResumenFinanzas> ObtenerResumenPeriodoActual();
        Respuesta<TResumenFinanzas> ObtenerResumenPorPeriodo(int idPeriodo);
        Respuesta<IEnumerable<TDetallePago>> ObtenerDetallesPorPeriodo(int idPeriodo);
        Respuesta<IEnumerable<TResumenFinanzas>> ObtenerResumenTodosPeriodos();
    }
}
