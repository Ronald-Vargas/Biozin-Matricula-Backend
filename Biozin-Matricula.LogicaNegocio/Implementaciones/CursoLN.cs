using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class CursoLN : ICursoLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<CursoLN> _logger;
        private readonly ILogActividadServicio _log;

        public CursoLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<CursoLN> logger, ILogActividadServicio log)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
            _log = log;
        }

        public Respuesta<int> Insertar(TCurso curso)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Cursos.ObtenerEntidad(y => y.Codigo == curso.Codigo);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Curso>(curso);
                    _unidadDeTrabajo.Cursos.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                    _log.Registrar("curso", $"Se creó el curso {curso.Nombre}", "📖");
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El curso ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Curso: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TCurso curso)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Cursos.ObtenerEntidad(y => y.IdCurso == curso.IdCurso);
                if (objDatos.ValorRetorno != null)
                {
                    var ofertasActivas = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidades(o => o.IdCurso == curso.IdCurso && o.Estado).ValorRetorno;
                    bool tieneOfertasActivas = ofertasActivas != null && ofertasActivas.Any();

                    // Bloquear desactivación si tiene ofertas activas
                    if (!curso.Estado && objDatos.ValorRetorno.Estado && tieneOfertasActivas)
                    {
                        resultado.lpError("No permitido", $"No se puede desactivar el curso porque tiene {ofertasActivas!.Count()} oferta(s) activa(s).");
                        return resultado;
                    }

                    // Bloquear cambio de modalidad (virtual/presencial) si tiene ofertas activas
                    if (curso.EsVirtual != objDatos.ValorRetorno.EsVirtual && tieneOfertasActivas)
                    {
                        resultado.lpError("No permitido", "No se puede cambiar la modalidad del curso (virtual/presencial) mientras tenga ofertas académicas activas.");
                        return resultado;
                    }

                    objDatos.ValorRetorno.Codigo = curso.Codigo;
                    objDatos.ValorRetorno.Creditos = curso.Creditos;
                    objDatos.ValorRetorno.Nombre = curso.Nombre;
                    objDatos.ValorRetorno.Descripcion = curso.Descripcion;
                    objDatos.ValorRetorno.Estado = curso.Estado;
                    objDatos.ValorRetorno.Precio = curso.Precio;
                    objDatos.ValorRetorno.idCursoRequisito = curso.idCursoRequisito;
                    objDatos.ValorRetorno.TieneLaboratorio = curso.TieneLaboratorio;
                    objDatos.ValorRetorno.PrecioLaboratorio = curso.PrecioLaboratorio;
                    objDatos.ValorRetorno.HorasDuracion = curso.HorasDuracion;
                    objDatos.ValorRetorno.EsVirtual = curso.EsVirtual;
                    _unidadDeTrabajo.Cursos.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El curso no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Curso: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TCurso curso)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Cursos.ObtenerEntidad(y => y.IdCurso == curso.IdCurso);
                if (objDatos.ValorRetorno != null)
                {
                    var enUso = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidades(o => o.IdCurso == curso.IdCurso).ValorRetorno;
                    if (enUso != null && enUso.Any())
                    {
                        resultado.lpError("No permitido", "No se puede eliminar el curso porque está asociado a una o más ofertas académicas.");
                        return resultado;
                    }

                    _unidadDeTrabajo.Cursos.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El curso no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Curso: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCurso>> Obtener(TCurso curso)
        {
            var resultado = new Respuesta<IEnumerable<TCurso>>();
            try
            {
                var datos = _unidadDeTrabajo.Cursos.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(curso.Nombre) || x.Nombre.Contains(curso.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCurso>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TCurso> Buscar(TCurso curso)
        {
            var resultado = new Respuesta<TCurso>();
            try
            {
                var datos = _unidadDeTrabajo.Cursos.ObtenerEntidad(x => x.IdCurso == curso.IdCurso);
                resultado.ValorRetorno = _mapper.Map<TCurso>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCurso>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TCurso>>();
            try
            {
                var datos = _unidadDeTrabajo.Cursos.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCurso>>(datos.ValorRetorno);
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
