using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class EstudianteLN : IEstudianteLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<EstudianteLN> _logger;

        public EstudianteLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<EstudianteLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TEstudiante estudiante)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Estudiantes.ObtenerEntidad(y => y.Cedula == estudiante.Cedula);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Estudiante>(estudiante);
                    entidad.FechaIngreso = DateTime.UtcNow;
                    _unidadDeTrabajo.Estudiantes.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
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
                    objDatos.ValorRetorno.CodigoEstudiante = estudiante.CodigoEstudiante;
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
