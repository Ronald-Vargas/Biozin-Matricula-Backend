using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface ICorreoServicio
    {

        Task EnviarCredencialesAsync(
            string correoDestino,
            string nombre,
            long carnet,
            string correoInstitucional,
            string password,
            string nombreUniversidad,
            string correoRemitente
        );

        Task EnviarCredencialesStaffAsync(
            string correoDestino,
            string nombre,
            string correoInstitucional,
            string password,
            string rol,
            string nombreUniversidad,
            string correoRemitente
        );

    }
}
