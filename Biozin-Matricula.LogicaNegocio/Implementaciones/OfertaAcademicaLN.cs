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
                var entidad = _mapper.Map<OfertaAcademica>(oferta);

                // Auto-generate Codigo if not provided
                if (string.IsNullOrEmpty(entidad.Codigo))
                {
                    entidad.Codigo = $"OFA-{entidad.IdPeriodo}-{entidad.IdCurso}-{DateTime.UtcNow:yyyyMMddHHmmss}";
                }

                // Check if Codigo already exists
                var objDatos = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(y => y.Codigo == entidad.Codigo);
                if (objDatos.ValorRetorno != null)
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "La oferta academica ya se encuentra registrada";
                    return resultado;
                }

                // Get Precio from Curso if not provided
                if (entidad.Precio == 0)
                {
                    var curso = _unidadDeTrabajo.Cursos.ObtenerEntidad(c => c.IdCurso == entidad.IdCurso);
                    if (curso.ValorRetorno != null)
                    {
                        entidad.Precio = curso.ValorRetorno.Precio;
                    }
                }

                entidad.FechaCreacion = DateTime.UtcNow;
                _unidadDeTrabajo.OfertasAcademicas.Insertar(entidad);
                resultado.ValorRetorno = _unidadDeTrabajo.Completar();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Oferta Academica: {0}{1}", ex.Message,
                    ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "");
                resultado.lpError("Error al Insertar", ex.InnerException?.Message ?? ex.Message);
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
                    objDatos.ValorRetorno.IdAula = oferta.IdAula;
                    objDatos.ValorRetorno.CupoMaximo = oferta.CupoMaximo;
                    objDatos.ValorRetorno.Matriculados = oferta.Matriculados;
                    objDatos.ValorRetorno.Precio = oferta.Precio;
                    objDatos.ValorRetorno.Estado = oferta.Estado;
                    _unidadDeTrabajo.OfertasAcademicas.Modificar(objDatos.ValorRetorno);

                    // Update DiasHorarios: remove existing and add new ones
                    var existentes = _unidadDeTrabajo.DiasHorarios.ObtenerEntidades(d => d.IdOferta == oferta.IdOferta);
                    if (existentes.ValorRetorno != null)
                    {
                        foreach (var dh in existentes.ValorRetorno)
                        {
                            _unidadDeTrabajo.DiasHorarios.Eliminar(dh);
                        }
                    }
                    foreach (var dh in oferta.DiasHorarios)
                    {
                        var nuevoDia = _mapper.Map<DiaHorario>(dh);
                        nuevoDia.IdOferta = oferta.IdOferta;
                        _unidadDeTrabajo.DiasHorarios.Insertar(nuevoDia);
                    }

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
                resultado.lpError("Error al Modificar", ex.InnerException?.Message ?? ex.Message);
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
                resultado.lpError("Error al Eliminar", ex.InnerException?.Message ?? ex.Message);
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
