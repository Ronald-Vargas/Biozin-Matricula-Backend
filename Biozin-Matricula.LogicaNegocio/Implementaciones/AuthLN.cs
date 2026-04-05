using BCrypt.Net;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class AuthLN : IAuthLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<AuthLN> _logger;

        public AuthLN(IUnidadTrabajoEF unidadDeTrabajo, ILogger<AuthLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _logger = logger;
        }

        public Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos)
        {
            var resultado = new Respuesta<object>();
            try
            {
                // Buscar en estudiantes por email institucional
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.EmailInstitucional == datos.Email)
                    .ValorRetorno;

                // Buscar en profesores si no se encontró en estudiantes
                var profesor = estudiante == null
                    ? _unidadDeTrabajo.Profesores
                        .ObtenerEntidad(p => p.EmailInstitucional == datos.Email)
                        .ValorRetorno
                    : null;

                if (estudiante == null && profesor == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                // Obtener la contraseña almacenada
                var contrasenaAlmacenada = estudiante != null
                    ? estudiante.Contrasena
                    : profesor!.Contrasena;

                // Verificar la contraseña temporal
                bool contrasenaValida = BCrypt.Net.BCrypt.Verify(datos.ContrasenaTemporal, contrasenaAlmacenada);

                if (!contrasenaValida)
                {
                    resultado.lpError("Error", "La contraseña temporal es incorrecta.");
                    return resultado;
                }

                // Validar que la nueva contraseña no sea igual a la temporal
                if (datos.NuevaContrasena == datos.ContrasenaTemporal)
                {
                    resultado.lpError("Error", "La nueva contraseña no puede ser igual a la temporal.");
                    return resultado;
                }

                // Hashear la nueva contraseña y actualizar
                var nuevaContrasenaHash = BCrypt.Net.BCrypt.HashPassword(datos.NuevaContrasena);

                if (estudiante != null)
                {
                    estudiante.Contrasena = nuevaContrasenaHash;
                    estudiante.RequiereCambioContrasena = false;
                    _unidadDeTrabajo.Estudiantes.Modificar(estudiante);
                }
                else
                {
                    profesor!.Contrasena = nuevaContrasenaHash;
                    profesor.RequiereCambioContrasena = false;
                    _unidadDeTrabajo.Profesores.Modificar(profesor);
                }

                _unidadDeTrabajo.Completar();

                resultado.blnError = false;
                resultado.strTituloRespuesta = "Éxito";
                resultado.strMensajeRespuesta = "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CambiarContrasenaTemporaria: {0}", ex.Message);
                resultado.lpError("Error", "Ocurrió un error al procesar la solicitud.");
            }
            return resultado;
        }
    }
}
