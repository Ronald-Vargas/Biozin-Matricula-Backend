using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IPortalProfesorLN
    {
        Respuesta<TPerfilProfesor> ObtenerPerfil(int idProfesor);
        Respuesta<List<TOfertaProfesor>> ObtenerMisCursos(int idProfesor);
        Respuesta<List<TEstudianteEnCurso>> ObtenerEstudiantesCurso(int idProfesor, int idOferta);
        Respuesta<bool> AsignarNota(int idProfesor, TAsignarNota solicitud);
    }
}
