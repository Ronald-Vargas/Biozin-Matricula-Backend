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

        Task EnviarComprobanteMatriculaAsync(
            string correoDestino,
            string nombre,
            long carnet,
            string codigoCurso,
            string nombreCurso,
            string nombrePeriodo,
            DateTime fechaMatricula,
            decimal monto,
            string nombreUniversidad,
            string correoRemitente
        );

        Task EnviarComprobantePagoAsync(
            string correoDestino,
            string nombre,
            long carnet,
            int numeroPago,
            string concepto,
            string nombrePeriodo,
            decimal monto,
            DateTime fechaPago,
            string nombreUniversidad,
            string correoRemitente
        );

    }
}
