using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAuthLN
    {
        Respuesta<TLoginRespuesta> Login(TLogin datos);
        Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos);
    }
}
