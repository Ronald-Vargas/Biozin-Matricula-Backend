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
    public class ProfesorLN : IProfesorLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfesorLN> _logger;
        private readonly ICorreoServicio _correo;
        private readonly IConfiguration _config;

        public ProfesorLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<ProfesorLN> logger, ICorreoServicio correo, IConfiguration config)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
            _correo = correo;
            _config = config;
        }

        public async Task<Respuesta<TCredencialesProfesor>> Insertar(TProfesor profesor)
        {
            var resultado = new Respuesta<TCredencialesProfesor>();
            try
            {
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.Cedula == profesor.Cedula);
                if (objDatos.ValorRetorno == null)
                {
                    // Generar email institucional con manejo de colisiones
                    var baseEmail = GeneradorCredenciales.GenerarBaseEmail(profesor.Nombre, profesor.ApellidoPaterno);
                    var email = GeneradorCredenciales.ConstruirEmail(baseEmail);
                    int sufijo = 2;
                    const int maxIntentosEmail = 100;
                    int intentosEmail = 0;
                    while (_unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.EmailInstitucional == email).ValorRetorno != null)
                    {
                        if (++intentosEmail >= maxIntentosEmail)
                        {
                            resultado.lpError("Error al Insertar", "No se pudo generar un email institucional único. Contacte al administrador.");
                            return resultado;
                        }
                        email = GeneradorCredenciales.ConstruirEmail(baseEmail, sufijo);
                        sufijo++;
                    }

                    // Generar contraseña y hashear
                    var contrasenaTxt = GeneradorCredenciales.GenerarContrasena();
                    var contrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaTxt);

                    var entidad = _mapper.Map<Profesor>(profesor);
                    entidad.EmailInstitucional = email;
                    entidad.Contrasena = contrasenaHash;
                    entidad.RequiereCambioContrasena = true;
                    entidad.FechaIngreso = DateTime.UtcNow;

                    _unidadDeTrabajo.Profesores.Insertar(entidad);
                    _unidadDeTrabajo.Completar();

                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"];
                    var urlCampus = ajustes?.sitioWeb ?? "";

                    await _correo.EnviarCredencialesStaffAsync(
                        profesor.EmailPersonal,
                        profesor.Nombre,
                        email,
                        contrasenaTxt,
                        "Profesor",
                        nombreUniversidad,
                        correoRemitente,
                        urlCampus
                    );

                    resultado.ValorRetorno = new TCredencialesProfesor
                    {
                        IdProfesor = entidad.IdProfesor,
                        EmailInstitucional = email,
                        ContrasenaGenerada = contrasenaTxt
                    };
                }
                else
                {
                    resultado.strMensajeRespuesta = "El profesor ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public async Task<Respuesta<object>> ReenviarCredenciales(int idProfesor)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var entidad = _unidadDeTrabajo.Profesores.ObtenerEntidad(p => p.IdProfesor == idProfesor).ValorRetorno;
                if (entidad == null)
                {
                    resultado.lpError("No encontrado", "El profesor no existe.");
                    return resultado;
                }

                var contrasenaTxt = GeneradorCredenciales.GenerarContrasena();
                entidad.Contrasena = BCrypt.Net.BCrypt.HashPassword(contrasenaTxt);
                entidad.RequiereCambioContrasena = true;
                _unidadDeTrabajo.Profesores.Modificar(entidad);
                _unidadDeTrabajo.Completar();

                var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"];
                var urlCampus = ajustes?.sitioWeb ?? "";

                await _correo.EnviarCredencialesStaffAsync(
                    entidad.EmailPersonal,
                    entidad.Nombre,
                    entidad.EmailInstitucional,
                    contrasenaTxt,
                    "Profesor",
                    nombreUniversidad,
                    correoRemitente,
                    urlCampus
                );

                resultado.strMensajeRespuesta = "Credenciales reenviadas correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ReenviarCredenciales Profesor: {0}", ex.Message);
                resultado.lpError("Error al reenviar credenciales", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TProfesor profesor)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.IdProfesor == profesor.IdProfesor);
                if (objDatos.ValorRetorno != null)
                {
                    // Bloquear desactivación si tiene ofertas activas
                    if (!profesor.Estado && objDatos.ValorRetorno.Estado)
                    {
                        var ofertasActivas = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidades(o => o.IdProfesor == profesor.IdProfesor && o.Estado).ValorRetorno;
                        if (ofertasActivas != null && ofertasActivas.Any())
                        {
                            resultado.lpError("No permitido", $"No se puede desactivar el profesor porque tiene {ofertasActivas.Count()} oferta(s) activa(s) asignadas.");
                            return resultado;
                        }
                    }

                    objDatos.ValorRetorno.Cedula = profesor.Cedula;
                    objDatos.ValorRetorno.Nombre = profesor.Nombre;
                    objDatos.ValorRetorno.ApellidoPaterno = profesor.ApellidoPaterno;
                    objDatos.ValorRetorno.ApellidoMaterno = profesor.ApellidoMaterno;
                    objDatos.ValorRetorno.FechaNacimiento = profesor.FechaNacimiento;
                    objDatos.ValorRetorno.Genero = profesor.Genero;
                    objDatos.ValorRetorno.Nacionalidad = profesor.Nacionalidad;
                    objDatos.ValorRetorno.EmailPersonal = profesor.EmailPersonal;
                    objDatos.ValorRetorno.Telefono = profesor.Telefono;
                    objDatos.ValorRetorno.Titulo = profesor.Titulo;
                    objDatos.ValorRetorno.Especialidad = profesor.Especialidad;
                    objDatos.ValorRetorno.Provincia = profesor.Provincia;
                    objDatos.ValorRetorno.Canton = profesor.Canton;
                    objDatos.ValorRetorno.Distrito = profesor.Distrito;
                    objDatos.ValorRetorno.Direccion = profesor.Direccion;
                    objDatos.ValorRetorno.EmailInstitucional = profesor.EmailInstitucional;
                    objDatos.ValorRetorno.Estado = profesor.Estado;
                    _unidadDeTrabajo.Profesores.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El profesor no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TProfesor profesor)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.IdProfesor == profesor.IdProfesor);
                if (objDatos.ValorRetorno != null)
                {
                    var enUso = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidades(o => o.IdProfesor == profesor.IdProfesor).ValorRetorno;
                    if (enUso != null && enUso.Any())
                    {
                        resultado.lpError("No permitido", "No se puede eliminar el profesor porque está asignado a una o más ofertas académicas.");
                        return resultado;
                    }

                    _unidadDeTrabajo.Profesores.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El profesor no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TProfesor>> Obtener(TProfesor profesor)
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(profesor.Nombre) || x.Nombre.Contains(profesor.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TProfesor> Buscar(TProfesor profesor)
        {
            var resultado = new Respuesta<TProfesor>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.ObtenerEntidad(x => x.IdProfesor == profesor.IdProfesor);
                resultado.ValorRetorno = _mapper.Map<TProfesor>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TProfesor>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TProfesor> Login(string email, string contrasena)
        {
            var resultado = new Respuesta<TProfesor>();
            try
            {
                var profesor = _unidadDeTrabajo.Profesores
                    .ObtenerEntidad(p => p.EmailInstitucional == email)
                    .ValorRetorno;

                if (profesor == null || !BCrypt.Net.BCrypt.Verify(contrasena, profesor.Contrasena))
                {
                    resultado.lpError("Error de autenticación", "Credenciales inválidas");
                    return resultado;
                }

                if (!profesor.Estado)
                {
                    resultado.lpError("Cuenta inactiva", "Su cuenta ha sido desactivada. Contacte al administrador.");
                    return resultado;
                }

                var perfil = _mapper.Map<TProfesor>(profesor);
                perfil.Contrasena = string.Empty;
                resultado.ValorRetorno = perfil;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Login Profesor: {0}", ex.Message);
                resultado.lpError("Error al iniciar sesión", ex.Message);
            }
            return resultado;
        }

        public Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var profesor = _unidadDeTrabajo.Profesores
                    .ObtenerEntidad(p => p.EmailInstitucional == datos.Email)
                    .ValorRetorno;

                if (profesor == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                if (profesor.RequiereCambioContrasena)
                {
                    if (!BCrypt.Net.BCrypt.Verify(datos.ContrasenaTemporal, profesor.Contrasena))
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

                profesor.Contrasena = BCrypt.Net.BCrypt.HashPassword(datos.NuevaContrasena);
                profesor.RequiereCambioContrasena = false;
                _unidadDeTrabajo.Profesores.Modificar(profesor);
                _unidadDeTrabajo.Completar();

                resultado.strTituloRespuesta = "Éxito";
                resultado.strMensajeRespuesta = "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CambiarContrasenaTemporaria Profesor: {0}", ex.Message);
                resultado.lpError("Error", "Ocurrió un error al procesar la solicitud.");
            }
            return resultado;
        }

        public async Task<Respuesta<object>> SolicitarRecuperacion(string email)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var profesor = _unidadDeTrabajo.Profesores
                    .ObtenerEntidad(p => p.EmailInstitucional == email)
                    .ValorRetorno;

                if (profesor == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                var codigo = RecuperacionCodigos.Generar(email);
                var nombreUniversidad = _config["Mail:NombreUniversidad"] ?? "Biozin";
                var correoRemitente = _config["Mail:Remitente"] ?? _config["Mail:Usuario"] ?? "";
                var correoDestino = !string.IsNullOrEmpty(profesor.EmailPersonal)
                    ? profesor.EmailPersonal
                    : profesor.EmailInstitucional!;

                await _correo.EnviarCodigoRecuperacionAsync(
                    correoDestino,
                    $"{profesor.Nombre} {profesor.ApellidoPaterno}",
                    codigo,
                    nombreUniversidad,
                    correoRemitente
                );

                resultado.strTituloRespuesta = "Código enviado";
                resultado.strMensajeRespuesta = "Se envió un código de recuperación a tu correo.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error SolicitarRecuperacion Profesor: {0}", ex.Message);
                resultado.lpError("Error", "No se pudo enviar el código. Intenta de nuevo.");
            }
            return resultado;
        }
    }
}
