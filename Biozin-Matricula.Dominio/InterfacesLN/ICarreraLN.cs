using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface ICarreraLN
    {
        Respuesta<int> Insertar(TCarrera carrera);
        Respuesta<int> Modificar(TCarrera carrera);
        Respuesta<bool> Eliminar(TCarrera carrera);
        Respuesta<IEnumerable<TCarrera>> Obtener(TCarrera carrera);
        Respuesta<TCarrera> Buscar(TCarrera carrera);
        Respuesta<IEnumerable<TCarrera>> Listar();
    }
}
