using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class CursoLN : ICategoriaLN
    {

        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteLN> _logger;

        public ClienteLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<ClienteLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TCliente cliente)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.TCliente.ObtenerEntidad(y => y.ClienteId == cliente.ClienteId);
                if (objDatos.ValorRetorno == null)
                {
                    var cli = _mapper.Map<Cliente>(cliente);
                    cli.FechaRegistro = DateTime.UtcNow;
                    cli.CreadoEn = DateTime.UtcNow;
                    _unidadDeTrabajo.TCliente.Insertar(cli);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El cliente ya se encuentra Registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Cliente: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<int> Modificar(TCliente cliente)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.TCliente.ObtenerEntidad(y => y.ClienteId == cliente.ClienteId);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Nombre = cliente.Nombre;
                    objDatos.ValorRetorno.Email = cliente.Email;
                    objDatos.ValorRetorno.Telefono = cliente.Telefono;
                    objDatos.ValorRetorno.Activo = cliente.Activo;
                    objDatos.ValorRetorno.TipoCedula = cliente.TipoCedula;
                    objDatos.ValorRetorno.ActualizadoEn = DateTime.UtcNow;
                    objDatos.ValorRetorno.ActualizadoPor = cliente.ActualizadoPor;
                    _unidadDeTrabajo.TCliente.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El cliente no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Cliente: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Eliminar(TCliente cliente)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.TCliente.ObtenerEntidad(y => y.ClienteId == cliente.ClienteId);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.TCliente.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else { resultado.ValorRetorno = false; resultado.strMensajeRespuesta = "No existe"; }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Cliente: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TCliente>> Obtener(TCliente clase)
        {
            var resultado = new Respuesta<IEnumerable<TCliente>>();
            try
            {
                var datos = _unidadDeTrabajo.TCliente.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(clase.Nombre) || x.Nombre.Contains(clase.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCliente>>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }

        public Respuesta<TCliente> Buscar(TCliente clase)
        {
            var resultado = new Respuesta<TCliente>();
            try
            {
                var datos = _unidadDeTrabajo.TCliente.ObtenerEntidad(x => x.ClienteId == clase.ClienteId);
                resultado.ValorRetorno = _mapper.Map<TCliente>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }

        public Respuesta<IEnumerable<TCliente>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TCliente>>();
            try
            {
                var datos = _unidadDeTrabajo.TCliente.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TCliente>>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }

    }
}
