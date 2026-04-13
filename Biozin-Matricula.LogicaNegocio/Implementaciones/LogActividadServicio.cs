using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class LogActividadServicio : ILogActividadServicio
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<LogActividadServicio> _logger;

        public LogActividadServicio(IUnidadTrabajoEF unidadDeTrabajo, ILogger<LogActividadServicio> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _logger = logger;
        }

        public void Registrar(string tipo, string descripcion, string icono)
        {
            try
            {
                _unidadDeTrabajo.LogActividades.Insertar(new LogActividad
                {
                    Tipo = tipo,
                    Descripcion = descripcion,
                    Icono = icono,
                    Fecha = DateTime.UtcNow
                });
                _unidadDeTrabajo.Completar();
            }
            catch (Exception ex)
            {
                // El log nunca debe romper la operación principal
                _logger.LogWarning("Error al registrar actividad: {0}", ex.Message);
            }
        }

        public Respuesta<List<TLogActividad>> ObtenerRecientes(int cantidad = 20)
        {
            var resultado = new Respuesta<List<TLogActividad>>();
            try
            {
                var logs = _unidadDeTrabajo.LogActividades
                    .ObtenerEntidades(_ => true)
                    .ValorRetorno?
                    .OrderByDescending(l => l.Fecha)
                    .Take(cantidad)
                    .Select(l => new TLogActividad
                    {
                        IdLog = l.IdLog,
                        Tipo = l.Tipo,
                        Descripcion = l.Descripcion,
                        Icono = l.Icono,
                        Fecha = l.Fecha
                    })
                    .ToList() ?? new List<TLogActividad>();

                resultado.ValorRetorno = logs;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerRecientes: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }
    }
}
