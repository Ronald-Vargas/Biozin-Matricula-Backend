using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IPeriodoLN
    {
        Respuesta<int> Insertar(TPeriodo periodo);
        Respuesta<int> Modificar(TPeriodo periodo);
        Respuesta<bool> Eliminar(TPeriodo periodo);
        Respuesta<IEnumerable<TPeriodo>> Obtener(TPeriodo periodo);
        Respuesta<TPeriodo> Buscar(TPeriodo periodo);
        Respuesta<IEnumerable<TPeriodo>> Listar();
    }
}
