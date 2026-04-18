using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;


namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class AulaLN : IAulaLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<AulaLN> _logger;

        public AulaLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<AulaLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TAula aula)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Aulas.ObtenerEntidad(y => y.NumeroAula == aula.NumeroAula);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Aula>(aula);
                    _unidadDeTrabajo.Aulas.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El aula ya se encuentra registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Aula: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TAula aula)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Aulas.ObtenerEntidad(y => y.IdAula == aula.IdAula);
                if (objDatos.ValorRetorno != null)
                {
                    // Bloquear desactivación si tiene ofertas activas
                    if (!aula.Activo && objDatos.ValorRetorno.Activo)
                    {
                        var ofertasActivas = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidades(o => o.IdAula == aula.IdAula && o.Estado).ValorRetorno;
                        if (ofertasActivas != null && ofertasActivas.Any())
                        {
                            resultado.lpError("No permitido", $"No se puede desactivar el aula porque está asignada a {ofertasActivas.Count()} oferta(s) activa(s).");
                            return resultado;
                        }
                    }

                    objDatos.ValorRetorno.NumeroAula = aula.NumeroAula;
                    objDatos.ValorRetorno.Capacidad = aula.Capacidad;
                    objDatos.ValorRetorno.Descripcion = aula.Descripcion;
                    objDatos.ValorRetorno.EsLaboratorio = aula.EsLaboratorio;
                    objDatos.ValorRetorno.Activo = aula.Activo;

                    _unidadDeTrabajo.Aulas.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El aula no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Aula: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TAula aula)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.Aulas.ObtenerEntidad(y => y.IdAula == aula.IdAula);
                if (objDatos.ValorRetorno != null)
                {
                    var enUso = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidades(o => o.IdAula == aula.IdAula).ValorRetorno;
                    if (enUso != null && enUso.Any())
                    {
                        resultado.lpError("No permitido", "No se puede eliminar el aula porque está asociada a una o más ofertas académicas.");
                        return resultado;
                    }

                    _unidadDeTrabajo.Aulas.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El aula no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Aula: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TAula>> Obtener(TAula aula)
        {
            var resultado = new Respuesta<IEnumerable<TAula>>();
            try
            {
                var datos = _unidadDeTrabajo.Aulas.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(aula.NumeroAula) || x.NumeroAula.Contains(aula.NumeroAula)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TAula>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TAula> Buscar(TAula aula)
        {
            var resultado = new Respuesta<TAula>();
            try
            {
                var datos = _unidadDeTrabajo.Aulas.ObtenerEntidad(x => x.IdAula == aula.IdAula);
                resultado.ValorRetorno = _mapper.Map<TAula>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TAula>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TAula>>();
            try
            {
                var datos = _unidadDeTrabajo.Aulas.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TAula>>(datos.ValorRetorno);
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
