using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class CarreraLN : ICarreraLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<CarreraLN> _logger;
        private readonly ILogActividadServicio _log;

        public CarreraLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<CarreraLN> logger, ILogActividadServicio log)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
            _log = log;
        }

        public Respuesta<int> Insertar(TCarrera carrera)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Carreras.ObtenerEntidad(y => y.Codigo == carrera.Codigo);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Carrera>(carrera);
                    _unidadDeTrabajo.Carreras.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                    _log.Registrar("carrera", $"Se creó la carrera {carrera.Nombre}", "🎓");
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La carrera ya se encuentra registrada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Carrera: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TCarrera carrera)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Carreras.ObtenerEntidad(y => y.IdCarrera == carrera.IdCarrera);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Codigo = carrera.Codigo;
                    objDatos.ValorRetorno.Duracion = carrera.Duracion;
                    objDatos.ValorRetorno.Nombre = carrera.Nombre;
                    objDatos.ValorRetorno.Descripcion = carrera.Descripcion;
                    objDatos.ValorRetorno.Estado = carrera.Estado;
                    _unidadDeTrabajo.Carreras.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La carrera no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Carrera: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TCarrera carrera)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Carreras.ObtenerEntidad(y => y.IdCarrera == carrera.IdCarrera);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.Carreras.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "La carrera no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Carrera: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCarrera>> Obtener(TCarrera carrera)
        {
            var resultado = new Respuesta<IEnumerable<TCarrera>>();
            try
            {
                var datos = _unidadDeTrabajo.Carreras.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(carrera.Nombre) || x.Nombre.Contains(carrera.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCarrera>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TCarrera> Buscar(TCarrera carrera)
        {
            var resultado = new Respuesta<TCarrera>();
            try
            {
                var datos = _unidadDeTrabajo.Carreras.ObtenerEntidad(x => x.IdCarrera == carrera.IdCarrera);
                resultado.ValorRetorno = _mapper.Map<TCarrera>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCarrera>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TCarrera>>();
            try
            {
                var datos = _unidadDeTrabajo.Carreras.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCarrera>>(datos.ValorRetorno);
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
