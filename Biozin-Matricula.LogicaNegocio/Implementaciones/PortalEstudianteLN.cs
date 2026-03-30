using System.Text.Json;
using BCrypt.Net;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class PortalEstudianteLN : IPortalEstudianteLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<PortalEstudianteLN> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public PortalEstudianteLN(IUnidadTrabajoEF unidadDeTrabajo, ILogger<PortalEstudianteLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _logger = logger;
        }




        public Respuesta<TPerfilEstudiante> Login(TLoginEstudiante login)
        {
            var resultado = new Respuesta<TPerfilEstudiante>();
            try
            {
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.EmailInstitucional == login.EmailInstitucional)
                    .ValorRetorno;

                if (estudiante == null)
                {
                    resultado.lpError("Error de autenticación", "Credenciales inválidas");
                    return resultado;
                }

                if (!BCrypt.Net.BCrypt.Verify(login.Contrasena, estudiante.Contrasena))
                {
                    resultado.lpError("Error de autenticación", "Credenciales inválidas");
                    return resultado;
                }

                var carrera = estudiante.IdCarrera.HasValue
                    ? _unidadDeTrabajo.Carreras.ObtenerEntidad(c => c.IdCarrera == estudiante.IdCarrera.Value).ValorRetorno
                    : null;

                resultado.ValorRetorno = new TPerfilEstudiante
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Nombre = estudiante.Nombre,
                    ApellidoPaterno = estudiante.ApellidoPaterno,
                    Carnet = estudiante.carnet,
                    IdCarrera = estudiante.IdCarrera,
                    NombreCarrera = carrera?.Nombre,
                    SemestreActual = estudiante.SemestreActual,
                    EmailInstitucional = estudiante.EmailInstitucional
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Login Estudiante: {0}", ex.Message);
                resultado.lpError("Error al iniciar sesión", ex.Message);
            }
            return resultado;
        }









        public Respuesta<TPerfilEstudiante> ObtenerPerfil(int idEstudiante)
        {
            var resultado = new Respuesta<TPerfilEstudiante>();
            try
            {
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                if (estudiante == null)
                {
                    resultado.lpError("Error", "Estudiante no encontrado");
                    return resultado;
                }

                var carrera = estudiante.IdCarrera.HasValue
                    ? _unidadDeTrabajo.Carreras.ObtenerEntidad(c => c.IdCarrera == estudiante.IdCarrera.Value).ValorRetorno
                    : null;

                resultado.ValorRetorno = new TPerfilEstudiante
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Nombre = estudiante.Nombre,
                    ApellidoPaterno = estudiante.ApellidoPaterno,
                    Carnet = estudiante.carnet,
                    IdCarrera = estudiante.IdCarrera,
                    NombreCarrera = carrera?.Nombre,
                    SemestreActual = estudiante.SemestreActual,
                    EmailInstitucional = estudiante.EmailInstitucional
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerPerfil: {0}", ex.Message);
                resultado.lpError("Error al obtener perfil", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TMatricularPeriodo> ObtenerOfertasDisponibles(int idEstudiante)
        {
            var resultado = new Respuesta<TMatricularPeriodo>();
            try
            {
                // Find active enrollment period
                var periodo = _unidadDeTrabajo.Periodos
                    .ObtenerEntidad(p => p.EstadoMatricula == true)
                    .ValorRetorno;

                if (periodo == null)
                {
                    resultado.strMensajeRespuesta = "No hay un período de matrícula activo";
                    return resultado;
                }

                // Get active offers for this period where there's still capacity
                var ofertas = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => o.IdPeriodo == periodo.IdPeriodo
                                        && o.Estado == true
                                        && o.Matriculados < o.CupoMaximo)
                    .ValorRetorno ?? Enumerable.Empty<OfertaAcademica>();

                // Get student's existing enrollments for this period to exclude them
                var matriculasExistentes = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var idsOfertasMatriculadas = matriculasExistentes
                    .Select(m => m.IdOferta)
                    .ToHashSet();

                var ofertasDisponibles = new List<TOfertaDisponible>();

                foreach (var oferta in ofertas)
                {
                    // Skip offers where student is already enrolled
                    if (idsOfertasMatriculadas.Contains(oferta.IdOferta))
                        continue;

                    var curso = _unidadDeTrabajo.Cursos
                        .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                        .ValorRetorno;

                    var profesor = _unidadDeTrabajo.Profesores
                        .ObtenerEntidad(p => p.IdProfesor == oferta.IdProfesor)
                        .ValorRetorno;

                    var aula = oferta.IdAula.HasValue
                        ? _unidadDeTrabajo.Aulas.ObtenerEntidad(a => a.IdAula == oferta.IdAula.Value).ValorRetorno
                        : null;

                    List<TDiaHorario>? horarios = null;
                    if (!string.IsNullOrEmpty(oferta.DiasHorarios))
                    {
                        horarios = JsonSerializer.Deserialize<List<TDiaHorario>>(oferta.DiasHorarios, _jsonOptions);
                    }

                    ofertasDisponibles.Add(new TOfertaDisponible
                    {
                        IdOferta = oferta.IdOferta,
                        Codigo = curso?.Codigo ?? "",
                        Nombre = curso?.Nombre ?? "",
                        Profesor = profesor != null ? $"{profesor.Nombre} {profesor.ApellidoPaterno}" : "",
                        Aula = aula?.NumeroAula,
                        Horario = horarios,
                        Creditos = curso?.Creditos ?? 0,
                        CupoMaximo = oferta.CupoMaximo,
                        Matriculados = oferta.Matriculados,
                        Precio = oferta.Precio
                    });
                }

                resultado.ValorRetorno = new TMatricularPeriodo
                {
                    IdPeriodo = periodo.IdPeriodo,
                    Nombre = periodo.Nombre,
                    FechaInicio = periodo.FechaInicio,
                    FechaFin = periodo.FechaFin,
                    FechaMatriculaFin = periodo.FechaMatriculaFin,
                    Ofertas = ofertasDisponibles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerOfertasDisponibles: {0}", ex.Message);
                resultado.lpError("Error al obtener ofertas", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> Matricular(int idEstudiante, int idOferta)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                // Validate active enrollment period
                var periodo = _unidadDeTrabajo.Periodos
                    .ObtenerEntidad(p => p.EstadoMatricula == true)
                    .ValorRetorno;

                if (periodo == null)
                {
                    resultado.lpError("Error", "No hay un período de matrícula activo");
                    return resultado;
                }

                // Validate offer exists, is active, and has capacity
                var oferta = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidad(o => o.IdOferta == idOferta && o.Estado == true && o.IdPeriodo == periodo.IdPeriodo)
                    .ValorRetorno;

                if (oferta == null)
                {
                    resultado.lpError("Error", "La oferta académica no está disponible");
                    return resultado;
                }

                if (oferta.Matriculados >= oferta.CupoMaximo)
                {
                    resultado.lpError("Error", "No hay cupos disponibles para esta oferta");
                    return resultado;
                }

                // Check student is not already enrolled
                var yaMatriculado = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidad(m => m.IdEstudiante == idEstudiante && m.IdOferta == idOferta)
                    .ValorRetorno;

                if (yaMatriculado != null)
                {
                    resultado.lpError("Error", "Ya está matriculado en esta oferta");
                    return resultado;
                }

                // Create enrollment
                var matricula = new Matricula
                {
                    IdEstudiante = idEstudiante,
                    IdOferta = idOferta,
                    FechaMatricula = DateTime.UtcNow,
                    Nota = null,
                    Estado = "en_curso"
                };

                _unidadDeTrabajo.Matriculas.Insertar(matricula);

                // Increment enrolled count
                oferta.Matriculados++;
                _unidadDeTrabajo.OfertasAcademicas.Modificar(oferta);

                _unidadDeTrabajo.Completar();

                // Create payment record for this enrollment
                var curso = _unidadDeTrabajo.Cursos
                    .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                    .ValorRetorno;

                var pago = new Pago
                {
                    IdMatricula = matricula.IdMatricula,
                    Concepto = $"Matrícula - {curso?.Nombre ?? "Curso"}",
                    Monto = oferta.Precio,
                    FechaVencimiento = periodo.FechaMatriculaFin.AddDays(30),
                    FechaPago = null,
                    Estado = "pendiente"
                };

                _unidadDeTrabajo.Pagos.Insertar(pago);
                _unidadDeTrabajo.Completar();

                resultado.ValorRetorno = true;
                resultado.strMensajeRespuesta = "Matrícula realizada exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Matricular: {0}", ex.Message);
                resultado.lpError("Error al matricular", ex.Message);
            }
            return resultado;
        }

        public Respuesta<List<THistorialSemestre>> ObtenerHistorial(int idEstudiante)
        {
            var resultado = new Respuesta<List<THistorialSemestre>>();
            try
            {
                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                // Group by period via offer
                var periodoMap = new Dictionary<int, (Periodo periodo, List<Matricula> matriculas)>();

                foreach (var mat in matriculas)
                {
                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;

                    if (oferta == null) continue;

                    if (!periodoMap.ContainsKey(oferta.IdPeriodo))
                    {
                        var periodo = _unidadDeTrabajo.Periodos
                            .ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo)
                            .ValorRetorno;

                        if (periodo == null) continue;

                        periodoMap[oferta.IdPeriodo] = (periodo, new List<Matricula>());
                    }

                    periodoMap[oferta.IdPeriodo].matriculas.Add(mat);
                }

                // Sort by period start date descending and build result
                var semestres = periodoMap.Values
                    .OrderByDescending(x => x.periodo.FechaInicio)
                    .ToList();

                var historial = new List<THistorialSemestre>();
                int semNum = semestres.Count;

                foreach (var (periodo, mats) in semestres)
                {
                    var cursos = new List<THistorialCurso>();

                    foreach (var mat in mats)
                    {
                        var oferta = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                            .ValorRetorno;

                        var curso = oferta != null
                            ? _unidadDeTrabajo.Cursos.ObtenerEntidad(c => c.IdCurso == oferta.IdCurso).ValorRetorno
                            : null;

                        cursos.Add(new THistorialCurso
                        {
                            Codigo = curso?.Codigo ?? "",
                            Nombre = curso?.Nombre ?? "",
                            Creditos = curso?.Creditos ?? 0,
                            Nota = mat.Nota,
                            Estado = mat.Estado
                        });
                    }

                    // Calculate average only for courses with grades
                    var cursosConNota = cursos.Where(c => c.Nota.HasValue).ToList();
                    decimal? promedio = cursosConNota.Any()
                        ? Math.Round(cursosConNota.Average(c => c.Nota!.Value), 2)
                        : null;

                    var labelPeriodo = periodo.EstadoMatricula
                        ? $"{periodo.Nombre} (En curso)"
                        : periodo.Nombre;

                    historial.Add(new THistorialSemestre
                    {
                        Label = $"Semestre {semNum}",
                        Periodo = labelPeriodo,
                        Promedio = promedio,
                        Cursos = cursos
                    });

                    semNum--;
                }

                resultado.ValorRetorno = historial;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerHistorial: {0}", ex.Message);
                resultado.lpError("Error al obtener historial", ex.Message);
            }
            return resultado;
        }

        public Respuesta<List<TPagoEstudiante>> ObtenerPagos(int idEstudiante)
        {
            var resultado = new Respuesta<List<TPagoEstudiante>>();
            try
            {
                // Get all enrollments for this student
                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var idsMatriculas = matriculas.Select(m => m.IdMatricula).ToList();

                var pagos = new List<TPagoEstudiante>();

                foreach (var idMatricula in idsMatriculas)
                {
                    var pagosMatricula = _unidadDeTrabajo.Pagos
                        .ObtenerEntidades(p => p.IdMatricula == idMatricula)
                        .ValorRetorno ?? Enumerable.Empty<Pago>();

                    var matricula = matriculas.First(m => m.IdMatricula == idMatricula);

                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == matricula.IdOferta)
                        .ValorRetorno;

                    var periodo = oferta != null
                        ? _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo).ValorRetorno
                        : null;

                    foreach (var pago in pagosMatricula)
                    {
                        // Update overdue status
                        var estado = pago.Estado;
                        if (estado == "pendiente" && pago.FechaVencimiento < DateTime.UtcNow)
                        {
                            estado = "vencido";
                        }

                        pagos.Add(new TPagoEstudiante
                        {
                            IdPago = pago.IdPago,
                            Concepto = pago.Concepto,
                            Monto = pago.Monto,
                            FechaVencimiento = pago.FechaVencimiento,
                            FechaPago = pago.FechaPago,
                            Estado = estado,
                            Periodo = periodo?.Nombre ?? ""
                        });
                    }
                }

                resultado.ValorRetorno = pagos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerPagos: {0}", ex.Message);
                resultado.lpError("Error al obtener pagos", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> RealizarPago(int idEstudiante, int idPago)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var pago = _unidadDeTrabajo.Pagos
                    .ObtenerEntidad(p => p.IdPago == idPago)
                    .ValorRetorno;

                if (pago == null)
                {
                    resultado.lpError("Error", "Pago no encontrado");
                    return resultado;
                }

                // Verify the payment belongs to this student
                var matricula = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidad(m => m.IdMatricula == pago.IdMatricula && m.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                if (matricula == null)
                {
                    resultado.lpError("Error", "No tiene permisos para realizar este pago");
                    return resultado;
                }

                if (pago.Estado == "pagado")
                {
                    resultado.lpError("Error", "Este pago ya fue realizado");
                    return resultado;
                }

                pago.Estado = "pagado";
                pago.FechaPago = DateTime.UtcNow;
                _unidadDeTrabajo.Pagos.Modificar(pago);
                _unidadDeTrabajo.Completar();

                resultado.ValorRetorno = true;
                resultado.strMensajeRespuesta = "Pago realizado exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error RealizarPago: {0}", ex.Message);
                resultado.lpError("Error al realizar pago", ex.Message);
            }
            return resultado;
        }
    }
}
