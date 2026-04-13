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
    public class EstudianteLN : IEstudianteLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<EstudianteLN> _logger;
        private readonly ICorreoServicio _correo;
        private readonly IConfiguration _config;
        private readonly ILogActividadServicio _log;

        public EstudianteLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<EstudianteLN> logger, ICorreoServicio correo, IConfiguration config, ILogActividadServicio log)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
            _correo = correo;
            _config = config;
            _log = log;
        }

        public async Task<Respuesta<TCredencialesEstudiante>> Insertar(TEstudiante estudiante)
        {
            var resultado = new Respuesta<TCredencialesEstudiante>();
            try
            {
                var objDatos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.Cedula == estudiante.Cedula);
                if (objDatos.ValorRetorno == null)
                {
                    // Generar email institucional con manejo de colisiones
                    var baseEmail = GeneradorCredenciales.GenerarBaseEmail(estudiante.Nombre, estudiante.ApellidoPaterno);
                    var email = GeneradorCredenciales.ConstruirEmailEstudiante(baseEmail);
                    int sufijo = 2;
                    while (_unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.EmailInstitucional == email).ValorRetorno != null)
                    {
                        email = GeneradorCredenciales.ConstruirEmailEstudiante(baseEmail, sufijo);
                        sufijo++;
                    }

                    // Generar carnet único
                    var carnet = GeneradorCredenciales.GenerarCarnet(DateTime.UtcNow.Year);
                    while (_unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.carnet == carnet).ValorRetorno != null)
                    {
                        carnet = GeneradorCredenciales.GenerarCarnet(DateTime.UtcNow.Year);
                    }

                    // Generar contraseña y hashear
                    var contrasenaTxt = GeneradorCredenciales.GenerarContrasena();
                    var contrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaTxt);

                    var entidad = _mapper.Map<Estudiante>(estudiante);
                    entidad.EmailInstitucional = email;
                    entidad.carnet = carnet;
                    entidad.Contrasena = contrasenaHash;
                    entidad.RequiereCambioContrasena = true;
                    entidad.FechaIngreso = DateTime.UtcNow;

                    _unidadDeTrabajo.Estudiantes.Insertar(entidad);
                    _unidadDeTrabajo.Completar();

                    _log.Registrar("estudiante", $"Se registró el estudiante {estudiante.Nombre} {estudiante.ApellidoPaterno}", "👨‍🎓");

                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"];

                    await _correo.EnviarCredencialesAsync(
                        estudiante.EmailPersonal,
                        estudiante.Nombre,
                        carnet,
                        email,
                        contrasenaTxt,
                        nombreUniversidad,
                        correoRemitente
                    );

                    resultado.ValorRetorno = new TCredencialesEstudiante
                    {
                        IdEstudiante = entidad.IdEstudiante,
                        Carnet = carnet,
                        EmailInstitucional = email,
                        ContrasenaGenerada = contrasenaTxt
                    };
                }
                else
                {
                    resultado.strMensajeRespuesta = "El estudiante ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Estudiante: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TEstudiante estudiante)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.IdEstudiante == estudiante.IdEstudiante);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Cedula = estudiante.Cedula;
                    objDatos.ValorRetorno.Nombre = estudiante.Nombre;
                    objDatos.ValorRetorno.ApellidoPaterno = estudiante.ApellidoPaterno;
                    objDatos.ValorRetorno.ApellidoMaterno = estudiante.ApellidoMaterno;
                    objDatos.ValorRetorno.FechaNacimiento = estudiante.FechaNacimiento;
                    objDatos.ValorRetorno.Genero = estudiante.Genero;
                    objDatos.ValorRetorno.Nacionalidad = estudiante.Nacionalidad;
                    objDatos.ValorRetorno.EmailInstitucional = estudiante.EmailInstitucional;
                    objDatos.ValorRetorno.EmailPersonal = estudiante.EmailPersonal;
                    objDatos.ValorRetorno.TelefonoMovil = estudiante.TelefonoMovil;
                    objDatos.ValorRetorno.TelefonoEmergencia = estudiante.TelefonoEmergencia;
                    objDatos.ValorRetorno.NombreContactoEmergencia = estudiante.NombreContactoEmergencia;
                    objDatos.ValorRetorno.Provincia = estudiante.Provincia;
                    objDatos.ValorRetorno.Canton = estudiante.Canton;
                    objDatos.ValorRetorno.Distrito = estudiante.Distrito;
                    objDatos.ValorRetorno.DireccionExacta = estudiante.DireccionExacta;
                    objDatos.ValorRetorno.IdCarrera = estudiante.IdCarrera;
                    objDatos.ValorRetorno.SemestreActual = estudiante.SemestreActual;
                    objDatos.ValorRetorno.EstadoEstudiante = estudiante.EstadoEstudiante;
                    objDatos.ValorRetorno.TipoBeca = estudiante.TipoBeca;
                    objDatos.ValorRetorno.CondicionSocioeconomica = estudiante.CondicionSocioeconomica;
                    objDatos.ValorRetorno.Trabaja = estudiante.Trabaja;
                    objDatos.ValorRetorno.ColegioProcedencia = estudiante.ColegioProcedencia;
                    objDatos.ValorRetorno.TipoColegio = estudiante.TipoColegio;
                    objDatos.ValorRetorno.AnioGraduacionColegio = estudiante.AnioGraduacionColegio;
                    objDatos.ValorRetorno.Discapacidad = estudiante.Discapacidad;
                    objDatos.ValorRetorno.TipoDiscapacidad = estudiante.TipoDiscapacidad;
                    objDatos.ValorRetorno.NecesitaAsistencia = estudiante.NecesitaAsistencia;
                    objDatos.ValorRetorno.Observaciones = estudiante.Observaciones;
                    _unidadDeTrabajo.Estudiantes.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El estudiante no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Estudiante: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TEstudiante estudiante)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.IdEstudiante == estudiante.IdEstudiante);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.Estudiantes.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El estudiante no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Estudiante: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TEstudiante>> Obtener(TEstudiante estudiante)
        {
            var resultado = new Respuesta<IEnumerable<TEstudiante>>();
            try
            {
                var datos = _unidadDeTrabajo.Estudiantes.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(estudiante.Nombre) || x.Nombre.Contains(estudiante.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TEstudiante>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TEstudiante> Buscar(TEstudiante estudiante)
        {
            var resultado = new Respuesta<TEstudiante>();
            try
            {
                var datos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(x => x.IdEstudiante == estudiante.IdEstudiante);
                resultado.ValorRetorno = _mapper.Map<TEstudiante>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TEstudiante>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TEstudiante>>();
            try
            {
                var datos = _unidadDeTrabajo.Estudiantes.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TEstudiante>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }
    }
}
