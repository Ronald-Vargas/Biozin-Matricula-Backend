using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface ICarreraCursoLN
    {
        Respuesta<int> Insertar(TCarreraCurso carreraCurso);
        Respuesta<int> InsertarMultiple(int idCarrera, IEnumerable<TCarreraCurso> asignaciones);
        Respuesta<int> Modificar(TCarreraCurso carreraCurso);
        Respuesta<bool> Eliminar(TCarreraCurso carreraCurso);
        Respuesta<bool> EliminarPorCarrera(int idCarrera);
        Respuesta<IEnumerable<TCarreraCurso>> Obtener(TCarreraCurso carreraCurso);
        Respuesta<TCarreraCurso> Buscar(TCarreraCurso carreraCurso);
        Respuesta<IEnumerable<TCarreraCurso>> Listar();
    }
}
