using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IPortalEstudianteLN
    {
        Respuesta<TPerfilEstudiante> ObtenerPerfil(int idEstudiante);
        Respuesta<TPerfilEstudiante> Login(TLoginEstudiante login);
        Respuesta<TMatricularPeriodo> ObtenerOfertasDisponibles(int idEstudiante);
        Respuesta<bool> Matricular(int idEstudiante, int idOferta);
        Respuesta<List<THistorialSemestre>> ObtenerHistorial(int idEstudiante);
        Respuesta<List<TPagoEstudiante>> ObtenerPagos(int idEstudiante);
        Respuesta<bool> RealizarPago(int idEstudiante, int idPago);
    }
}
