using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class ProfesorLN : IProfesorLN
    {

        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfesorLN> _logger;

        public ProfesorLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<ProfesorLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }

        public Respuesta<int> Insertar(TProfesor profesor)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.TProfesor.ObtenerEntidad(y => y.ProfesorId == profesor.ProfesorId);
                if (objDatos.ValorRetorno == null)
                {
                    var cli = _mapper.Map<Profesor>(profesor);
                    _unidadDeTrabajo.TProfesor.Insertar(cli);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El profesor ya se encuentra Registrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }






        public Respuesta<int> Modificar(TProfesor profesor)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.TProfesor.ObtenerEntidad(y => y.ProfesorId == profesor.ProfesorId);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Nombre = profesor.Nombre;
                    objDatos.ValorRetorno.Email = profesor.Email;
                    objDatos.ValorRetorno.Telefono = profesor.Telefono;
                    objDatos.ValorRetorno.Activo = profesor.Activo;
                    objDatos.ValorRetorno.TipoCedula = cliprofesorente.TipoCedula;
                    objDatos.ValorRetorno.ActualizadoPor = cliente.ActualizadoPor;
                    _unidadDeTrabajo.TProfesor.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El profesor no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }











        public Respuesta<bool> Eliminar(TProfesor profesor)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var objDatos = _unidadDeTrabajo.TProfesor.ObtenerEntidad(y => y.ProfesorId == profesor.ProfesorId);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.TProfesor.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else { resultado.ValorRetorno = false; resultado.strMensajeRespuesta = "No existe"; }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }










        public Respuesta<IEnumerable<TProfesor>> Obtener(TProfesor clase)
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.TProfesor.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(clase.Nombre) || x.Nombre.Contains(clase.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }










        public Respuesta<TProfesor> Buscar(TProfesor clase)
        {
            var resultado = new Respuesta<TProfesor>();
            try
            {
                var datos = _unidadDeTrabajo.TProfesor.ObtenerEntidad(x => x.ProfesorId == clase.ProfesorId);
                resultado.ValorRetorno = _mapper.Map<TProfesor>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }








        public Respuesta<IEnumerable<TProfesor>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.TProfesor.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
            }
            catch (Exception ex) { _logger.LogError(ex.Message); resultado.lpError("Error", ex.Message); }
            return resultado;
        }

    }
}
