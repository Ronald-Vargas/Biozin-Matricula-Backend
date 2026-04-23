using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IEstudianteLN
    {
        Task<Respuesta<TCredencialesEstudiante>> Insertar(TEstudiante estudiante);
        Respuesta<int> Modificar(TEstudiante estudiante);
        Respuesta<bool> Eliminar(TEstudiante estudiante);
        Respuesta<IEnumerable<TEstudiante>> Obtener(TEstudiante estudiante);
        Respuesta<TEstudiante> Buscar(TEstudiante estudiante);
        Respuesta<IEnumerable<TEstudiante>> Listar();
        Task<Respuesta<object>> ReenviarCredenciales(int idEstudiante);
    }
}
