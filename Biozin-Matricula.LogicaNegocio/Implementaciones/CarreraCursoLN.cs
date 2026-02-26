using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class CarreraCursoLN : ICarreraCursoLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<CarreraCursoLN> _logger;

        public CarreraCursoLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<CarreraCursoLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TCarreraCurso carreraCurso)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.CarreraCursos.ObtenerEntidad(y =>
                    y.IdCarrera == carreraCurso.IdCarrera && y.IdCurso == carreraCurso.IdCurso);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<CarreraCurso>(carreraCurso);
                    _unidadDeTrabajo.CarreraCursos.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La relacion carrera-curso ya se encuentra registrada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar CarreraCurso: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TCarreraCurso carreraCurso)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.CarreraCursos.ObtenerEntidad(y => y.Id == carreraCurso.Id);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.IdCarrera = carreraCurso.IdCarrera;
                    objDatos.ValorRetorno.IdCurso = carreraCurso.IdCurso;
                    objDatos.ValorRetorno.Semestre = carreraCurso.Semestre;
                    objDatos.ValorRetorno.EsObligatorio = carreraCurso.EsObligatorio;
                    _unidadDeTrabajo.CarreraCursos.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La relacion carrera-curso no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar CarreraCurso: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TCarreraCurso carreraCurso)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.CarreraCursos.ObtenerEntidad(y => y.Id == carreraCurso.Id);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.CarreraCursos.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "La relacion carrera-curso no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar CarreraCurso: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCarreraCurso>> Obtener(TCarreraCurso carreraCurso)
        {
            var resultado = new Respuesta<IEnumerable<TCarreraCurso>>();
            try
            {
                var datos = _unidadDeTrabajo.CarreraCursos.ObtenerEntidades(x =>
                    x.IdCarrera == carreraCurso.IdCarrera);
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCarreraCurso>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TCarreraCurso> Buscar(TCarreraCurso carreraCurso)
        {
            var resultado = new Respuesta<TCarreraCurso>();
            try
            {
                var datos = _unidadDeTrabajo.CarreraCursos.ObtenerEntidad(x => x.Id == carreraCurso.Id);
                resultado.ValorRetorno = _mapper.Map<TCarreraCurso>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCarreraCurso>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TCarreraCurso>>();
            try
            {
                var datos = _unidadDeTrabajo.CarreraCursos.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCarreraCurso>>(datos.ValorRetorno);
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
