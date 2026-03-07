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

        public CursoLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<CursoLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
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
                    objDatos.ValorRetorno.Codigo = curso.Codigo;
                    objDatos.ValorRetorno.Creditos = curso.Creditos;
                    objDatos.ValorRetorno.Nombre = curso.Nombre;
                    objDatos.ValorRetorno.Descripcion = curso.Descripcion;
                    objDatos.ValorRetorno.Estado = curso.Estado;
                    objDatos.ValorRetorno.Precio = curso.Precio;
                    objDatos.ValorRetorno.TieneLaboratorio = curso.TieneLaboratorio;
                    objDatos.ValorRetorno.PrecioLaboratorio = curso.PrecioLaboratorio;
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
