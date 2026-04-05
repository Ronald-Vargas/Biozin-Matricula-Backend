using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAuthLN
    {
        Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos);
    }
}
