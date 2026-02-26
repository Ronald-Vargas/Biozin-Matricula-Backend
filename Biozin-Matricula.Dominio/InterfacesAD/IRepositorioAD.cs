using System.Linq.Expressions;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesAD
{
    public interface IRepositorioAD<T> where T : class
    {
        Respuesta<T> ObtenerEntidad(Expression<Func<T, bool>> filtro);
        Respuesta<IEnumerable<T>> ObtenerEntidades(Expression<Func<T, bool>> filtro);
        Respuesta<IEnumerable<T>> Listar();
        void Insertar(T entidad);
        void Modificar(T entidad);
        void Eliminar(T entidad);
    }
}
