using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class PeriodoLN : IPeriodoLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<PeriodoLN> _logger;

        public PeriodoLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<PeriodoLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TPeriodo periodo)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Periodos.ObtenerEntidad(y => y.IdPeriodo == periodo.IdPeriodo);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Periodo>(periodo);
                    _unidadDeTrabajo.Periodos.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El periodo ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Periodo: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TPeriodo periodo)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Periodos.ObtenerEntidad(y => y.IdPeriodo == periodo.IdPeriodo);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Nombre = periodo.Nombre;
                    objDatos.ValorRetorno.FechaInicio = periodo.FechaInicio;
                    objDatos.ValorRetorno.FechaFin = periodo.FechaFin;
                    objDatos.ValorRetorno.FechaMatriculaInicio = periodo.FechaMatriculaInicio;
                    objDatos.ValorRetorno.FechaMatriculaFin = periodo.FechaMatriculaFin;
                    objDatos.ValorRetorno.EstadoMatricula = periodo.EstadoMatricula;
                    _unidadDeTrabajo.Periodos.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El periodo no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Periodo: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TPeriodo periodo)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Periodos.ObtenerEntidad(y => y.IdPeriodo == periodo.IdPeriodo);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.Periodos.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El periodo no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Periodo: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TPeriodo>> Obtener(TPeriodo periodo)
        {
            var resultado = new Respuesta<IEnumerable<TPeriodo>>();
            try
            {
                var datos = _unidadDeTrabajo.Periodos.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(periodo.Nombre) || x.Nombre.Contains(periodo.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TPeriodo>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TPeriodo> Buscar(TPeriodo periodo)
        {
            var resultado = new Respuesta<TPeriodo>();
            try
            {
                var datos = _unidadDeTrabajo.Periodos.ObtenerEntidad(x => x.IdPeriodo == periodo.IdPeriodo);
                resultado.ValorRetorno = _mapper.Map<TPeriodo>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TPeriodo>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TPeriodo>>();
            try
            {
                var datos = _unidadDeTrabajo.Periodos.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TPeriodo>>(datos.ValorRetorno);
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
