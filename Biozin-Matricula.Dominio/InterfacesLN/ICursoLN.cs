using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface ICursoLN
    {
        Respuesta<int> Insertar(TCurso curso);
        Respuesta<int> Modificar(TCurso curso);
        Respuesta<bool> Eliminar(TCurso curso);
        Respuesta<IEnumerable<TCurso>> Obtener(TCurso curso);
        Respuesta<TCurso> Buscar(TCurso curso);
        Respuesta<IEnumerable<TCurso>> Listar();
    }
}
