using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IProfesorLN
    {
        Task<Respuesta<TCredencialesProfesor>> Insertar(TProfesor profesor);
        Respuesta<int> Modificar(TProfesor profesor);
        Respuesta<bool> Eliminar(TProfesor profesor);
        Respuesta<IEnumerable<TProfesor>> Obtener(TProfesor profesor);
        Respuesta<TProfesor> Buscar(TProfesor profesor);
        Respuesta<IEnumerable<TProfesor>> Listar();
    }
}
