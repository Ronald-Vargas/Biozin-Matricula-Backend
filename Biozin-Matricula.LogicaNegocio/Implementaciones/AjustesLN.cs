using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class AjustesLN : IAjustesLN
    {

        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IMapper _mapper;
        private readonly ILogger<AjustesLN> _logger;


        public AjustesLN(IUnidadTrabajoEF unidadDeTrabajo, IMapper mapper, ILogger<AjustesLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _mapper = mapper;
            _logger = logger;
        }





        public Respuesta<int> Insertar(TAjustes ajustes)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Ajustes.ObtenerEntidad(y => y.idAjuste == ajustes.idAjuste);
                if (objDatos.ValorRetorno == null)
                {
                    var entidad = _mapper.Map<Ajustes>(ajustes);
                    _unidadDeTrabajo.Ajustes.Insertar(entidad);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "Los ajustes ya se encuentran registrados";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Insertar Ajustes: {0}", ex.Message);
                resultado.lpError("Error al Insertar", ex.Message);
            }
            return resultado;
        }





        public Respuesta<int> Modificar(TAjustes ajustes)
        {
            var resultado = new Respuesta<int>();
            try
            {
                var objDatos = _unidadDeTrabajo.Ajustes.ObtenerEntidad(y => y.idAjuste == ajustes.idAjuste);
                if (objDatos.ValorRetorno != null)
                {
                    objDatos.ValorRetorno.idAjuste = ajustes.idAjuste;
                    objDatos.ValorRetorno.nombreUniversidad = ajustes.nombreUniversidad;
                    objDatos.ValorRetorno.sitioWeb = ajustes.sitioWeb;
                    objDatos.ValorRetorno.correoInstitucional = ajustes.correoInstitucional;
                    objDatos.ValorRetorno.telefono = ajustes.telefono;
                    objDatos.ValorRetorno.direccion = ajustes.direccion;
                    objDatos.ValorRetorno.provincia = ajustes.provincia;
                    objDatos.ValorRetorno.canton = ajustes.canton;
                    objDatos.ValorRetorno.distrito = ajustes.distrito;
                    _unidadDeTrabajo.Ajustes.Modificar(objDatos.ValorRetorno);
                    resultado.ValorRetorno = _unidadDeTrabajo.Completar();
                }
                else
                {
                    resultado.ValorRetorno = -1;
                    resultado.strMensajeRespuesta = "El Ajuste no existe";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Modificar Ajustes: {0}", ex.Message);
                resultado.lpError("Error al Modificar", ex.Message);
            }
            return resultado;
        }





        public Respuesta<TAjustes> Obtener()
        {
            var resultado = new Respuesta<TAjustes>();

            try
            {
                var datos = _unidadDeTrabajo.Ajustes.Listar();

                var ajuste = datos.ValorRetorno.FirstOrDefault();

                resultado.ValorRetorno = _mapper.Map<TAjustes>(ajuste);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                resultado.lpError("Error", ex.Message);
            }

            return resultado;
        }





        public Respuesta<IEnumerable<TAjustes>> Listar()
        {
            var resultado = new Respuesta<IEnumerable<TAjustes>>();
            try
            {
                var datos = _unidadDeTrabajo.Ajustes.Listar();
                resultado.ValorRetorno = _mapper.Map<IEnumerable<TAjustes>>(datos.ValorRetorno);
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
