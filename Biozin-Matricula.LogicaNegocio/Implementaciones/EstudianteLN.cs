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
                if (estudiante.IdsCarreras == null || estudiante.IdsCarreras.Count == 0)
                {
                    resultado.lpError("Error al Insertar", "Debe seleccionar al menos una carrera.");
                    return resultado;
                }

                var objDatos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.Cedula == estudiante.Cedula);
                if (objDatos.ValorRetorno == null)
                {
                    var baseEmail = GeneradorCredenciales.GenerarBaseEmail(estudiante.Nombre, estudiante.ApellidoPaterno);
                    var email = GeneradorCredenciales.ConstruirEmailEstudiante(baseEmail);
                    int sufijo = 2;
                    const int maxIntentosEmail = 100;
                    int intentosEmail = 0;
                    while (_unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.EmailInstitucional == email).ValorRetorno != null)
                    {
                        if (++intentosEmail >= maxIntentosEmail)
                        {
                            resultado.lpError("Error al Insertar", "No se pudo generar un email institucional único. Contacte al administrador.");
                            return resultado;
                        }
                        email = GeneradorCredenciales.ConstruirEmailEstudiante(baseEmail, sufijo);
                        sufijo++;
                    }

                    var carnet = GeneradorCredenciales.GenerarCarnet(DateTime.UtcNow.Year);
                    const int maxIntentosCarnet = 100;
                    int intentosCarnet = 0;
                    while (_unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.carnet == carnet).ValorRetorno != null)
                    {
                        if (++intentosCarnet >= maxIntentosCarnet)
                        {
                            resultado.lpError("Error al Insertar", "No se pudo generar un carnet único. Contacte al administrador.");
                            return resultado;
                        }
                        carnet = GeneradorCredenciales.GenerarCarnet(DateTime.UtcNow.Year);
                    }

                    var contrasenaTxt = GeneradorCredenciales.GenerarContrasena();
                    var contrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasenaTxt);

                    var entidad = _mapper.Map<Estudiante>(estudiante);
                    entidad.EmailInstitucional = email;
                    entidad.carnet = carnet;
                    entidad.Contrasena = contrasenaHash;
                    entidad.RequiereCambioContrasena = true;
                    entidad.FechaIngreso = DateTime.UtcNow;
                    // Mantener id_carrera con el primer valor para compatibilidad con la columna existente
                    entidad.IdCarrera = estudiante.IdsCarreras.First();

                    _unidadDeTrabajo.Estudiantes.Insertar(entidad);
                    _unidadDeTrabajo.Completar();

                    // Insertar relaciones de carrera
                    foreach (var idCarrera in estudiante.IdsCarreras.Distinct())
                    {
                        _unidadDeTrabajo.EstudianteCarreras.Insertar(new EstudianteCarrera
                        {
                            IdEstudiante = entidad.IdEstudiante,
                            IdCarrera = idCarrera
                        });
                    }
                    _unidadDeTrabajo.Completar();

                    _log.Registrar("estudiante", $"Se registró el estudiante {estudiante.Nombre} {estudiante.ApellidoPaterno}", "👨‍🎓");

                    resultado.ValorRetorno = new TCredencialesEstudiante
                    {
                        IdEstudiante = entidad.IdEstudiante,
                        Carnet = carnet,
                        EmailInstitucional = email,
                        ContrasenaGenerada = contrasenaTxt
                    };

                    try
                    {
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
                    }
                    catch (Exception exCorreo)
                    {
                        _logger.LogWarning("No se pudo enviar el correo al estudiante {0}: {1}", email, exCorreo.Message);
                        resultado.strMensajeRespuesta = "Estudiante creado, pero no se pudo enviar el correo con las credenciales.";
                    }
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
                if (estudiante.IdsCarreras == null || estudiante.IdsCarreras.Count == 0)
                {
                    resultado.lpError("Error al Modificar", "Debe seleccionar al menos una carrera.");
                    return resultado;
                }

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
                    _unidadDeTrabajo.Completar();

                    // Sincronizar carreras: eliminar las actuales e insertar las nuevas
                    var carrerasActuales = _unidadDeTrabajo.EstudianteCarreras
                        .ObtenerEntidades(ec => ec.IdEstudiante == estudiante.IdEstudiante)
                        .ValorRetorno ?? Enumerable.Empty<EstudianteCarrera>();

                    foreach (var ec in carrerasActuales)
                        _unidadDeTrabajo.EstudianteCarreras.Eliminar(ec);

                    foreach (var idCarrera in estudiante.IdsCarreras.Distinct())
                    {
                        _unidadDeTrabajo.EstudianteCarreras.Insertar(new EstudianteCarrera
                        {
                            IdEstudiante = estudiante.IdEstudiante,
                            IdCarrera = idCarrera
                        });
                    }

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
                    (estudiante.IdEstudiante == 0 || x.IdEstudiante == estudiante.IdEstudiante) &&
                    (string.IsNullOrEmpty(estudiante.Nombre) || x.Nombre.Contains(estudiante.Nombre)));
                var lista = (_mapper.Map<IEnumerable<TEstudiante>>(datos.ValorRetorno) ?? Enumerable.Empty<TEstudiante>()).ToList();
                foreach (var t in lista) EnriquecerAcademico(t);
                resultado.ValorRetorno = lista;
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
                var t = _mapper.Map<TEstudiante>(datos.ValorRetorno);
                if (t != null) EnriquecerAcademico(t);
                resultado.ValorRetorno = t;
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
                var lista = (_mapper.Map<IEnumerable<TEstudiante>>(datos.ValorRetorno) ?? Enumerable.Empty<TEstudiante>()).ToList();
                foreach (var t in lista) EnriquecerAcademico(t);
                resultado.ValorRetorno = lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        private void EnriquecerAcademico(TEstudiante t)
        {
            // Cargar carreras desde la tabla de relación
            var relaciones = _unidadDeTrabajo.EstudianteCarreras
                .ObtenerEntidades(ec => ec.IdEstudiante == t.IdEstudiante)
                .ValorRetorno ?? Enumerable.Empty<EstudianteCarrera>();

            t.IdsCarreras = relaciones.Select(ec => ec.IdCarrera).ToList();
            t.Carreras = new List<TCarreraResumen>();

            foreach (var rel in relaciones)
            {
                var carrera = _unidadDeTrabajo.Carreras.ObtenerEntidad(c => c.IdCarrera == rel.IdCarrera).ValorRetorno;
                if (carrera != null)
                    t.Carreras.Add(new TCarreraResumen { IdCarrera = carrera.IdCarrera, Codigo = carrera.Codigo, Nombre = carrera.Nombre });
            }

            // Matrículas del estudiante
            var matriculas = _unidadDeTrabajo.Matriculas
                .ObtenerEntidades(m => m.IdEstudiante == t.IdEstudiante)
                .ValorRetorno ?? Enumerable.Empty<Biozin_Matricula.Dominio.Entidades.Matricula>();

            var idsCursosAprobados = new HashSet<int>();
            foreach (var mat in matriculas.Where(m => m.Estado == "aprobado"))
            {
                var oferta = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(o => o.IdOferta == mat.IdOferta).ValorRetorno;
                if (oferta == null) continue;
                idsCursosAprobados.Add(oferta.IdCurso);
                var curso = _unidadDeTrabajo.Cursos.ObtenerEntidad(c => c.IdCurso == oferta.IdCurso).ValorRetorno;
                if (curso != null) t.CreditosAprobados += curso.Creditos;
            }

            // Créditos totales: suma de todas las carreras (sin duplicar cursos compartidos)
            var idsCursosContados = new HashSet<int>();
            foreach (var idCarrera in t.IdsCarreras)
            {
                var cursosCarrera = _unidadDeTrabajo.CarreraCursos
                    .ObtenerEntidades(cc => cc.IdCarrera == idCarrera)
                    .ValorRetorno ?? Enumerable.Empty<Biozin_Matricula.Dominio.Entidades.CarreraCurso>();

                foreach (var cc in cursosCarrera)
                {
                    if (idsCursosContados.Contains(cc.IdCurso)) continue;
                    idsCursosContados.Add(cc.IdCurso);
                    var curso = _unidadDeTrabajo.Cursos.ObtenerEntidad(c => c.IdCurso == cc.IdCurso).ValorRetorno;
                    if (curso != null) t.CreditosTotales += curso.Creditos;
                }
            }

            // Semestre actual
            var periodosDistintos = new HashSet<int>();
            foreach (var mat in matriculas)
            {
                var ofertaMat = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(o => o.IdOferta == mat.IdOferta).ValorRetorno;
                if (ofertaMat != null) periodosDistintos.Add(ofertaMat.IdPeriodo);
            }
            if (periodosDistintos.Count > 0)
                t.SemestreActual = periodosDistintos.Count;
        }
    }
}
