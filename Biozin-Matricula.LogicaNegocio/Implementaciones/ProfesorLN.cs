using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

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
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.Cedula == profesor.Cedula);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Profesor>(profesor);
                    entidad.FechaIngreso = DateTime.UtcNow;
                    _unidadDeTrabajo.Profesores.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El profesor ya se encuentra registrado";
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
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.Id == profesor.Id);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.Cedula = profesor.Cedula;
                    objDatos.ValorRetorno.Nombre = profesor.Nombre;
                    objDatos.ValorRetorno.ApellidoPaterno = profesor.ApellidoPaterno;
                    objDatos.ValorRetorno.ApellidoMaterno = profesor.ApellidoMaterno;
                    objDatos.ValorRetorno.FechaNacimiento = profesor.FechaNacimiento;
                    objDatos.ValorRetorno.Genero = profesor.Genero;
                    objDatos.ValorRetorno.Nacionalidad = profesor.Nacionalidad;
                    objDatos.ValorRetorno.EmailPersonal = profesor.EmailPersonal;
                    objDatos.ValorRetorno.Telefono = profesor.Telefono;
                    objDatos.ValorRetorno.Titulo = profesor.Titulo;
                    objDatos.ValorRetorno.Especialidad = profesor.Especialidad;
                    objDatos.ValorRetorno.CursosAsignados = profesor.CursosAsignados;
                    objDatos.ValorRetorno.Provincia = profesor.Provincia;
                    objDatos.ValorRetorno.Canton = profesor.Canton;
                    objDatos.ValorRetorno.Distrito = profesor.Distrito;
                    objDatos.ValorRetorno.Direccion = profesor.Direccion;
                    objDatos.ValorRetorno.Codigo = profesor.Codigo;
                    objDatos.ValorRetorno.EmailInstitucional = profesor.EmailInstitucional;
                    objDatos.ValorRetorno.Estado = profesor.Estado;
                    _unidadDeTrabajo.Profesores.Modificar(objDatos.ValorRetorno);
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
                var objDatos = _unidadDeTrabajo.Profesores.ObtenerEntidad(y => y.Id == profesor.Id);
                if (objDatos.ValorRetorno != null)
                {
                    _unidadDeTrabajo.Profesores.Eliminar(objDatos.ValorRetorno);
                    _unidadDeTrabajo.Completar();
                    resultado.ValorRetorno = true;
                }
                else
                {
                    resultado.ValorRetorno = false;
                    resultado.strMensajeRespuesta = "El profesor no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Eliminar Profesor: {0}", ex.Message);
                resultado.lpError("Error al Eliminar", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TProfesor>> Obtener(TProfesor profesor)
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.ObtenerEntidades(x =>
                    (string.IsNullOrEmpty(profesor.Nombre) || x.Nombre.Contains(profesor.Nombre)));
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TProfesor> Buscar(TProfesor profesor)
        {
            var resultado = new Respuesta<TProfesor>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.ObtenerEntidad(x => x.Id == profesor.Id);
                resultado.ValorRetorno = _mapper.Map<TProfesor>(datos.ValorRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TProfesor>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TProfesor>>();
            try
            {
                var datos = _unidadDeTrabajo.Profesores.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TProfesor>>(datos.ValorRetorno);
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
