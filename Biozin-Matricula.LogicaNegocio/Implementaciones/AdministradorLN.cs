using AutoMapper;
using BCrypt.Net;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class AdministradorLN : IAdministradorLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<AdministradorLN> _logger;
        private readonly ICorreoServicio _correo;
        private readonly IConfiguration _config;

        public AdministradorLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<AdministradorLN> logger, ICorreoServicio correo, IConfiguration config)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
            _correo = correo;
            _config = config;
        }

        public async Task<Respuesta<int>> Insertar(TAdministrador administrador)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Administradores.ObtenerEntidad(y => y.Identificacion == administrador.Identificacion);
                if (objDatos.ValorRetorno == null)
                {
                    
                    var entidad = _mapper.Map<Administrador>(administrador);
                    var baseEmail = GeneradorCredenciales.GenerarBaseEmailDesdeNombreCompleto(administrador.NombreCompleto);
                    entidad.EmailInstitucional = GeneradorCredenciales.ConstruirEmailAdministrador(baseEmail);

                    // Generar contraseña y hashear
                    var contrasenaTxt = GeneradorCredenciales.GenerarContrasena();
                    var contrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaTxt);

                    entidad.Contraseña = contrasenaHash;
                    entidad.RequiereCambioContrasena = true;
                    _unidadDeTrabajo.Administradores.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();

                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"];

                    await _correo.EnviarCredencialesStaffAsync(
                        administrador.Correo,
                        administrador.NombreCompleto,
                        entidad.EmailInstitucional,
                        contrasenaTxt,
                        "Administrador",
                        nombreUniversidad,
                        correoRemitente
                    );
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El administrador ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Administrador: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }








        public Respuesta<int> Modificar(TAdministrador administrador)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Administradores.ObtenerEntidad(y => y.IdAdministrador == administrador.IdAdministrador);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Identificacion = administrador.Identificacion;
                    objDatos.ValorRetorno.NombreCompleto = administrador.NombreCompleto;
                    objDatos.ValorRetorno.EmailInstitucional = administrador.EmailInstitucional;
                    objDatos.ValorRetorno.Correo = administrador.Correo;
                    objDatos.ValorRetorno.Telefono = administrador.Telefono;
                    objDatos.ValorRetorno.Activo = administrador.Activo;

                    _unidadDeTrabajo.Administradores.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El administrador no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Administrador: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }










        public Respuesta<bool> Eliminar(TAdministrador administrador)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Administradores.ObtenerEntidad(y => y.IdAdministrador == administrador.IdAdministrador);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.Administradores.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El administrador no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Administrador: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }









        public Respuesta<IEnumerable<TAdministrador>> Obtener(TAdministrador administrador)
        {
            var resultado = new Respuesta<IEnumerable<TAdministrador>>();
            try
            {
                var datos = _unidadDeTrabajo.Administradores.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(administrador.Identificacion) || x.Identificacion.Contains(administrador.Identificacion)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TAdministrador>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }









        public Respuesta<TAdministrador> Buscar(TAdministrador administrador)
        {
            var resultado = new Respuesta<TAdministrador>();
            try
            {
                var datos = _unidadDeTrabajo.Administradores.ObtenerEntidad(x => x.IdAdministrador == administrador.IdAdministrador);
                resultado.ValorRetorno = _mapper.Map<TAdministrador>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }






        public Respuesta<IEnumerable<TAdministrador>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TAdministrador>>();
            try
            {
                var datos = _unidadDeTrabajo.Administradores.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TAdministrador>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var admin = _unidadDeTrabajo.Administradores
                    .ObtenerEntidad(a => a.EmailInstitucional == datos.Email)
                    .ValorRetorno;

                if (admin == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                if (admin.RequiereCambioContrasena)
                {
                    if (!BCrypt.Net.BCrypt.Verify(datos.ContrasenaTemporal, admin.Contraseña))
                    {
                        resultado.lpError("Error", "La contraseña temporal es incorrecta.");
                        return resultado;
                    }
                }
                else
                {
                    if (!RecuperacionCodigos.Validar(datos.Email, datos.ContrasenaTemporal))
                    {
                        resultado.lpError("Error", "El código de recuperación es inválido o ha expirado.");
                        return resultado;
                    }
                }

                admin.Contraseña = BCrypt.Net.BCrypt.HashPassword(datos.NuevaContrasena);
                admin.RequiereCambioContrasena = false;
                _unidadDeTrabajo.Administradores.Modificar(admin);
                _unidadDeTrabajo.Completar();

                resultado.strTituloRespuesta = "Éxito";
                resultado.strMensajeRespuesta = "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CambiarContrasenaTemporaria Administrador: {0}", ex.Message);
                resultado.lpError("Error", "Ocurrió un error al procesar la solicitud.");
            }
            return resultado;
        }

        public async Task<Respuesta<object>> SolicitarRecuperacion(string email)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var admin = _unidadDeTrabajo.Administradores
                    .ObtenerEntidad(a => a.EmailInstitucional == email)
                    .ValorRetorno;

                if (admin == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                var codigo = RecuperacionCodigos.Generar(email);
                var nombreUniversidad = _config["Mail:NombreUniversidad"] ?? "Biozin";
                var correoRemitente = _config["Mail:Remitente"] ?? _config["Mail:Usuario"] ?? "";

                await _correo.EnviarCodigoRecuperacionAsync(
                    admin.EmailInstitucional,
                    admin.NombreCompleto,
                    codigo,
                    nombreUniversidad,
                    correoRemitente
                );

                resultado.strTituloRespuesta = "Código enviado";
                resultado.strMensajeRespuesta = "Se envió un código de recuperación a tu correo.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error SolicitarRecuperacion Administrador: {0}", ex.Message);
                resultado.lpError("Error", "No se pudo enviar el código. Intenta de nuevo.");
            }
            return resultado;
        }

        public Respuesta<TAdministrador> Login(string email, string contrasena)
        {
            var resultado = new Respuesta<TAdministrador>();
            try
            {
                var admin = _unidadDeTrabajo.Administradores
                    .ObtenerEntidad(a => a.EmailInstitucional == email)
                    .ValorRetorno;

                if (admin == null || !BCrypt.Net.BCrypt.Verify(contrasena, admin.Contraseña))
                {
                    resultado.lpError("Error de autenticación", "Credenciales inválidas");
                    return resultado;
                }

                if (!admin.Activo)
                {
                    resultado.lpError("Cuenta inactiva", "Su cuenta ha sido desactivada. Contacte al administrador.");
                    return resultado;
                }

                var perfil = _mapper.Map<TAdministrador>(admin);
                perfil.Contraseña = string.Empty;
                resultado.ValorRetorno = perfil;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Login Administrador: {0}", ex.Message);
                resultado.lpError("Error al iniciar sesión", ex.Message);
            }
            return resultado;
        }
    }
}
