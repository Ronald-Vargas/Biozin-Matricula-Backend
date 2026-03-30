using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAdministradorLN
    {
        Respuesta<int> Insertar(TAdministrador administrador);
        Respuesta<int> Modificar(TAdministrador administrador);
        Respuesta<bool> Eliminar(TAdministrador administrador);
        Respuesta<IEnumerable<TAdministrador>> Obtener(TAdministrador administrador);
        Respuesta<TAdministrador> Buscar(TAdministrador administrador);
        Respuesta<IEnumerable<TAdministrador>> Listar();
        Respuesta<TAdministrador> Login(string email, string contrasena);
    }
}
