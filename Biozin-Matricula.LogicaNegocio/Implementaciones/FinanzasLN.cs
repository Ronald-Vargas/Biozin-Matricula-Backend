using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class FinanzasLN : IFinanzasLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<FinanzasLN> _logger;

        public FinanzasLN(IUnidadTrabajoEF unidadDeTrabajo, IDateTimeProvider dateTimeProvider, ILogger<FinanzasLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public Respuesta<TResumenFinanzas> ObtenerResumenPeriodoActual()
        {
            var resultado = new Respuesta<TResumenFinanzas>();
            try
            {
                var ahora = _dateTimeProvider.Ahora;
                var periodos = _unidadDeTrabajo.Periodos.Listar().ValorRetorno ?? [];
                var periodoActual = periodos.FirstOrDefault(p => p.FechaInicio <= ahora && p.FechaFin >= ahora);

                if (periodoActual == null)
                {
                    resultado.lpError("Sin período activo", "No hay un período académico activo en la fecha actual.");
                    return resultado;
                }

                resultado.ValorRetorno = CalcularResumen(periodoActual.IdPeriodo, periodoActual.Nombre, ahora);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerResumenPeriodoActual: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TResumenFinanzas> ObtenerResumenPorPeriodo(int idPeriodo)
        {
            var resultado = new Respuesta<TResumenFinanzas>();
            try
            {
                var periodo = _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == idPeriodo).ValorRetorno;
                if (periodo == null)
                {
                    resultado.lpError("No encontrado", "El período especificado no existe.");
                    return resultado;
                }

                var ahora = _dateTimeProvider.Ahora;
                resultado.ValorRetorno = CalcularResumen(idPeriodo, periodo.Nombre, ahora);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerResumenPorPeriodo: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TDetallePago>> ObtenerDetallesPorPeriodo(int idPeriodo)
        {
            var resultado = new Respuesta<IEnumerable<TDetallePago>>();
            try
            {
                var periodo = _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == idPeriodo).ValorRetorno;
                if (periodo == null)
                {
                    resultado.lpError("No encontrado", "El período especificado no existe.");
                    return resultado;
                }

                var ahora = _dateTimeProvider.Ahora;
                var ofertas = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidades(o => o.IdPeriodo == idPeriodo).ValorRetorno ?? [];
                var ofertaIds = ofertas.Select(o => o.IdOferta).ToHashSet();

                var matriculas = _unidadDeTrabajo.Matriculas.Listar().ValorRetorno ?? [];
                var matriculasDelPeriodo = matriculas.Where(m => ofertaIds.Contains(m.IdOferta)).ToList();
                var matriculaIds = matriculasDelPeriodo.Select(m => m.IdMatricula).ToHashSet();

                var pagos = _unidadDeTrabajo.Pagos.Listar().ValorRetorno ?? [];
                var pagosDelPeriodo = pagos.Where(p => matriculaIds.Contains(p.IdMatricula)).ToList();

                var estudiantes = _unidadDeTrabajo.Estudiantes.Listar().ValorRetorno ?? [];
                var estudiantesMap = estudiantes.ToDictionary(e => e.IdEstudiante);

                var matriculasMap = matriculasDelPeriodo.ToDictionary(m => m.IdMatricula);

                var detalles = pagosDelPeriodo.Select(pago =>
                {
                    matriculasMap.TryGetValue(pago.IdMatricula, out var matricula);
                    var estudiante = matricula != null && estudiantesMap.TryGetValue(matricula.IdEstudiante, out var est) ? est : null;

                    var estadoReal = pago.Estado;
                    if (estadoReal == "pendiente" && pago.FechaVencimiento < ahora)
                        estadoReal = "vencido";

                    return new TDetallePago
                    {
                        IdPago = pago.IdPago,
                        Concepto = pago.Concepto,
                        Monto = pago.Monto,
                        Estado = estadoReal,
                        FechaVencimiento = pago.FechaVencimiento,
                        FechaPago = pago.FechaPago,
                        NombreEstudiante = estudiante != null ? $"{estudiante.Nombre} {estudiante.ApellidoPaterno} {estudiante.ApellidoMaterno}".Trim() : "Desconocido",
                        CarnetEstudiante = estudiante?.carnet.ToString() ?? string.Empty,
                        NombrePeriodo = periodo.Nombre
                    };
                }).OrderByDescending(d => d.FechaVencimiento).ToList();

                resultado.ValorRetorno = detalles;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerDetallesPorPeriodo: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        public Respuesta<IEnumerable<TResumenFinanzas>> ObtenerResumenTodosPeriodos()
        {
            var resultado = new Respuesta<IEnumerable<TResumenFinanzas>>();
            try
            {
                var ahora = _dateTimeProvider.Ahora;
                var periodos = _unidadDeTrabajo.Periodos.Listar().ValorRetorno ?? [];
                var resumenes = periodos
                    .OrderByDescending(p => p.FechaInicio)
                    .Select(p => CalcularResumen(p.IdPeriodo, p.Nombre, ahora))
                    .ToList();

                resultado.ValorRetorno = resumenes;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerResumenTodosPeriodos: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        private TResumenFinanzas CalcularResumen(int idPeriodo, string nombrePeriodo, DateTime ahora)
        {
            var ofertas = _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidades(o => o.IdPeriodo == idPeriodo).ValorRetorno ?? [];
            var ofertaIds = ofertas.Select(o => o.IdOferta).ToHashSet();

            var matriculas = _unidadDeTrabajo.Matriculas.Listar().ValorRetorno ?? [];
            var matriculasDelPeriodo = matriculas.Where(m => ofertaIds.Contains(m.IdOferta)).ToList();
            var matriculaIds = matriculasDelPeriodo.Select(m => m.IdMatricula).ToHashSet();

            var pagos = _unidadDeTrabajo.Pagos.Listar().ValorRetorno ?? [];
            var pagosDelPeriodo = pagos.Where(p => matriculaIds.Contains(p.IdMatricula)).ToList();

            decimal totalRecaudado = 0, totalPendiente = 0, totalVencido = 0;
            int cantPagados = 0, cantPendientes = 0, cantVencidos = 0;

            foreach (var pago in pagosDelPeriodo)
            {
                var estado = pago.Estado;
                if (estado == "pendiente" && pago.FechaVencimiento < ahora)
                    estado = "vencido";

                switch (estado)
                {
                    case "pagado":
                        totalRecaudado += pago.Monto;
                        cantPagados++;
                        break;
                    case "vencido":
                        totalVencido += pago.Monto;
                        cantVencidos++;
                        break;
                    default:
                        totalPendiente += pago.Monto;
                        cantPendientes++;
                        break;
                }
            }

            return new TResumenFinanzas
            {
                IdPeriodo = idPeriodo,
                NombrePeriodo = nombrePeriodo,
                TotalRecaudado = totalRecaudado,
                TotalPendiente = totalPendiente,
                TotalVencido = totalVencido,
                CantidadPagados = cantPagados,
                CantidadPendientes = cantPendientes,
                CantidadVencidos = cantVencidos,
                TotalMatriculas = matriculasDelPeriodo.Count
            };
        }
    }
}
