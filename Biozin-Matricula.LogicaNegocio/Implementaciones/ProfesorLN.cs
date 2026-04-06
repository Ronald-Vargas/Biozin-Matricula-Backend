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
                    while (_unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.EmailInstitucional == email).ValorRetorno != null)
                    {
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

                    await _correo.EnviarCredencialesStaffAsync(
                        profesor.EmailPersonal,
                        profesor.Nombre,
                        email,
                        contrasenaTxt,
                        "Profesor",
                        nombreUniversidad,
                        correoRemitente
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

        public Respuesta<int> Modificar(TProfesor profesor)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.IdProfesor == profesor.IdProfesor);
                if (objDatos.ValorRetorno != null)
                {
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
                    objDatos.ValorRetorno.CursosAsignados = profesor.CursosAsignados;
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
    }
}
