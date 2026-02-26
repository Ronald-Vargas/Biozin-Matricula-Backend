using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class OfertaAcademicaLN : IOfertaAcademicaLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<OfertaAcademicaLN> _logger;

        public OfertaAcademicaLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<OfertaAcademicaLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TOfertaAcademica oferta)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(y => y.Codigo == oferta.Codigo);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<OfertaAcademica>(oferta);
                    entidad.FechaCreacion = DateTime.UtcNow;
                    _unidadDeTrabajo.OfertasAcademicas.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La oferta academica ya se encuentra registrada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Oferta Academica: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TOfertaAcademica oferta)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(y => y.IdOferta == oferta.IdOferta);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Codigo = oferta.Codigo;
                    objDatos.ValorRetorno.IdPeriodo = oferta.IdPeriodo;
                    objDatos.ValorRetorno.IdCurso = oferta.IdCurso;
                    objDatos.ValorRetorno.IdProfesor = oferta.IdProfesor;
                    objDatos.ValorRetorno.Dias = oferta.Dias;
                    objDatos.ValorRetorno.Horario = oferta.Horario;
                    objDatos.ValorRetorno.Aula = oferta.Aula;
                    objDatos.ValorRetorno.CupoMaximo = oferta.CupoMaximo;
                    objDatos.ValorRetorno.Matriculados = oferta.Matriculados;
                    objDatos.ValorRetorno.Precio = oferta.Precio;
                    objDatos.ValorRetorno.Estado = oferta.Estado;
                    _unidadDeTrabajo.OfertasAcademicas.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La oferta academica no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Oferta Academica: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TOfertaAcademica oferta)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(y => y.IdOferta == oferta.IdOferta);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.OfertasAcademicas.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "La oferta academica no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Oferta Academica: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TOfertaAcademica>> Obtener(TOfertaAcademica oferta)
        {
            var resultado = new Respuesta<IEnumerable<TOfertaAcademica>>();
            try
            {
                var datos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(oferta.Codigo) || x.Codigo.Contains(oferta.Codigo)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TOfertaAcademica>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TOfertaAcademica> Buscar(TOfertaAcademica oferta)
        {
            var resultado = new Respuesta<TOfertaAcademica>();
            try
            {
                var datos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(x => x.IdOferta == oferta.IdOferta);
                resultado.ValorRetorno = _mapper.Map<TOfertaAcademica>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TOfertaAcademica>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TOfertaAcademica>>();
            try
            {
                var datos = _unidadDeTrabajo.OfertasAcademicas.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TOfertaAcademica>>(datos.ValorRetorno);
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
