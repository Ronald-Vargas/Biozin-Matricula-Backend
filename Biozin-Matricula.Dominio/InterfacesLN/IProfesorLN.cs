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
        Respuesta<TProfesor> Login(string email, string contrasena);
        Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos);
        Task<Respuesta<object>> SolicitarRecuperacion(string email);
    }
}
