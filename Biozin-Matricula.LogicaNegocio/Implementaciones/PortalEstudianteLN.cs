using System.Text.Json;
using BCrypt.Net;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class PortalEstudianteLN : IPortalEstudianteLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<PortalEstudianteLN> _logger;
        private readonly ICorreoServicio _correo;
        private readonly IConfiguration _config;
        private readonly ILogActividadServicio _log;
        private readonly IDateTimeProvider _tiempo;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public PortalEstudianteLN(IUnidadTrabajoEF unidadDeTrabajo, ILogger<PortalEstudianteLN> logger, ICorreoServicio correo, IConfiguration config, ILogActividadServicio log, IDateTimeProvider tiempo)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _logger = logger;
            _correo = correo;
            _config = config;
            _log = log;
            _tiempo = tiempo;
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
                    EmailInstitucional = estudiante.EmailInstitucional,
                    RequiereCambioContrasena = estudiante.RequiereCambioContrasena
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Login Estudiante: {0}", ex.Message);
                resultado.lpError("Error al iniciar sesión", ex.Message);
            }
            return resultado;
        }

        public Respuesta<object> CambiarContrasenaTemporaria(TCambioContrasena datos)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.EmailInstitucional == datos.Email)
                    .ValorRetorno;

                if (estudiante == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                if (estudiante.RequiereCambioContrasena)
                {
                    // Primer inicio de sesión: validar contraseña temporal con BCrypt
                    if (!BCrypt.Net.BCrypt.Verify(datos.ContrasenaTemporal, estudiante.Contrasena))
                    {
                        resultado.lpError("Error", "La contraseña temporal es incorrecta.");
                        return resultado;
                    }
                }
                else
                {
                    // Recuperación por olvido: validar código de recuperación
                    if (!RecuperacionCodigos.Validar(datos.Email, datos.ContrasenaTemporal))
                    {
                        resultado.lpError("Error", "El código de recuperación es inválido o ha expirado.");
                        return resultado;
                    }
                }

                estudiante.Contrasena = BCrypt.Net.BCrypt.HashPassword(datos.NuevaContrasena);
                estudiante.RequiereCambioContrasena = false;
                _unidadDeTrabajo.Estudiantes.Modificar(estudiante);
                _unidadDeTrabajo.Completar();

                resultado.strTituloRespuesta = "Éxito";
                resultado.strMensajeRespuesta = "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CambiarContrasenaTemporaria: {0}", ex.Message);
                resultado.lpError("Error", "Ocurrió un error al procesar la solicitud.");
            }
            return resultado;
        }

        public async Task<Respuesta<object>> SolicitarRecuperacion(string email)
        {
            var resultado = new Respuesta<object>();
            try
            {
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.EmailInstitucional == email)
                    .ValorRetorno;

                if (estudiante == null)
                {
                    resultado.lpError("Error", "No se encontró una cuenta con ese correo.");
                    return resultado;
                }

                var codigo = RecuperacionCodigos.Generar(email);
                var nombreUniversidad = _config["Mail:NombreUniversidad"] ?? "Biozin";
                var correoRemitente = _config["Mail:Remitente"] ?? _config["Mail:Usuario"] ?? "";

                // Enviar al correo personal del estudiante si existe, si no al institucional
                var correoDestino = !string.IsNullOrEmpty(estudiante.EmailPersonal)
                    ? estudiante.EmailPersonal
                    : estudiante.EmailInstitucional;

                await _correo.EnviarCodigoRecuperacionAsync(
                    correoDestino!,
                    $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                    codigo,
                    nombreUniversidad,
                    correoRemitente
                );

                resultado.strTituloRespuesta = "Código enviado";
                resultado.strMensajeRespuesta = "Se envió un código de recuperación a tu correo.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error SolicitarRecuperacion Estudiante: {0}", ex.Message);
                resultado.lpError("Error", "No se pudo enviar el código. Intenta de nuevo.");
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

                // Créditos totales de la carrera (suma de créditos de todos sus cursos)
                int creditosTotales = 0;
                if (estudiante.IdCarrera.HasValue)
                {
                    var cursosCarrera = _unidadDeTrabajo.CarreraCursos
                        .ObtenerEntidades(cc => cc.IdCarrera == estudiante.IdCarrera.Value)
                        .ValorRetorno ?? Enumerable.Empty<CarreraCurso>();

                    foreach (var cc in cursosCarrera)
                    {
                        var curso = _unidadDeTrabajo.Cursos
                            .ObtenerEntidad(c => c.IdCurso == cc.IdCurso)
                            .ValorRetorno;
                        if (curso != null)
                            creditosTotales += curso.Creditos;
                    }
                }

                // Calcular créditos según el estado académico de la matrícula
                var todasLasMatriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                // Período activo: aquel marcado con EstadoMatricula = true
                var periodoActual = _unidadDeTrabajo.Periodos
                    .ObtenerEntidad(p => p.EstadoMatricula == true)
                    .ValorRetorno;

                int creditosAprobados = 0;
                int creditosMatriculados = 0;
                int creditosEnCurso = 0;

                foreach (var mat in todasLasMatriculas)
                {
                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;

                    if (oferta == null) continue;

                    var curso = _unidadDeTrabajo.Cursos
                        .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                        .ValorRetorno;

                    if (curso == null) continue;

                    // Total de todos los créditos llevados (todos los períodos)
                    creditosMatriculados += curso.Creditos;

                    if (periodoActual != null && oferta.IdPeriodo == periodoActual.IdPeriodo)
                        // Período actual → en curso
                        creditosEnCurso += curso.Creditos;
                    else if (mat.Estado == "aprobado")
                        // Períodos pasados → solo aprobados
                        creditosAprobados += curso.Creditos;
                }

                resultado.ValorRetorno = new TPerfilEstudiante
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Nombre = estudiante.Nombre,
                    ApellidoPaterno = estudiante.ApellidoPaterno,
                    Carnet = estudiante.carnet,
                    IdCarrera = estudiante.IdCarrera,
                    NombreCarrera = carrera?.Nombre,
                    SemestreActual = estudiante.SemestreActual,
                    EmailInstitucional = estudiante.EmailInstitucional,
                    CreditosAprobados = creditosAprobados,
                    CreditosMatriculados = creditosMatriculados,
                    CreditosEnCurso = creditosEnCurso,
                    CreditosTotales = creditosTotales
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
                // Período de matrícula activo
                var periodo = _unidadDeTrabajo.Periodos
                    .ObtenerEntidad(p => p.EstadoMatricula == true)
                    .ValorRetorno;

                if (periodo == null)
                {
                    resultado.strMensajeRespuesta = "No hay un período de matrícula activo";
                    return resultado;
                }

                var ahora = _tiempo.Ahora;
                if (ahora < periodo.FechaMatriculaInicio || ahora > periodo.FechaMatriculaFin)
                {
                    resultado.strMensajeRespuesta =
                        $"La matrícula estará disponible del {periodo.FechaMatriculaInicio:dd/MM/yyyy} al {periodo.FechaMatriculaFin:dd/MM/yyyy}";
                    return resultado;
                }

                // Carrera del estudiante
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                if (estudiante?.IdCarrera == null)
                {
                    resultado.strMensajeRespuesta = "El estudiante no tiene una carrera asignada";
                    return resultado;
                }

                // Cursos que pertenecen al plan de la carrera del estudiante
                var cursosCarrera = _unidadDeTrabajo.CarreraCursos
                    .ObtenerEntidades(cc => cc.IdCarrera == estudiante.IdCarrera.Value)
                    .ValorRetorno ?? Enumerable.Empty<CarreraCurso>();

                var idsCursosCarrera = cursosCarrera.Select(cc => cc.IdCurso).ToHashSet();

                // Ofertas del período activo con cupo, limitadas a cursos de la carrera
                var ofertas = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => o.IdPeriodo == periodo.IdPeriodo
                                       && o.Estado == true
                                       && o.Matriculados < o.CupoMaximo
                                       && idsCursosCarrera.Contains(o.IdCurso))
                    .ValorRetorno ?? Enumerable.Empty<OfertaAcademica>();

                // Todas las matrículas del estudiante en cualquier período
                var todasLasMatriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                // Separar matrículas del período actual de las de períodos anteriores
                var idsOfertasPeriodoActual = ofertas.Select(o => o.IdOferta).ToHashSet();

                var matriculasPeriodoActual = todasLasMatriculas
                    .Where(m => idsOfertasPeriodoActual.Contains(m.IdOferta))
                    .ToList();

                var matriculasPeriodosAnteriores = todasLasMatriculas
                    .Where(m => !idsOfertasPeriodoActual.Contains(m.IdOferta))
                    .ToList();

                // Cursos en los que el estudiante ya está inscrito en el período actual
                // → se excluyen todas las ofertas del mismo curso (cubre múltiples secciones)
                var idsCursosEnPeriodoActual = new HashSet<int>();
                foreach (var mat in matriculasPeriodoActual)
                {
                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat != null)
                        idsCursosEnPeriodoActual.Add(ofertaMat.IdCurso);
                }

                // Cursos ya completados: pagados en períodos anteriores
                // → el estudiante ya los llevó, no pueden volver a aparecer
                var idsCursosPagados = new HashSet<int>();
                foreach (var mat in matriculasPeriodosAnteriores)
                {
                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat == null) continue;

                    var pago = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula)
                        .ValorRetorno;

                    if (pago?.Estado == "pagado")
                        idsCursosPagados.Add(ofertaMat.IdCurso);
                }

                var ofertasDisponibles = new List<TOfertaDisponible>();

                foreach (var oferta in ofertas)
                {
                    var curso = _unidadDeTrabajo.Cursos
                        .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                        .ValorRetorno;

                    if (curso == null) continue;

                    // Ya matriculado en este curso en el período actual (cualquier sección)
                    if (idsCursosEnPeriodoActual.Contains(curso.IdCurso))
                        continue;

                    // Ya llevó el curso (pagado en período anterior)
                    if (idsCursosPagados.Contains(curso.IdCurso))
                        continue;

                    // Prerequisito no completado
                    if (curso.idCursoRequisito.HasValue && !idsCursosPagados.Contains(curso.idCursoRequisito.Value))
                        continue;

                    var profesor = _unidadDeTrabajo.Profesores
                        .ObtenerEntidad(p => p.IdProfesor == oferta.IdProfesor)
                        .ValorRetorno;

                    var aula = oferta.IdAula.HasValue
                        ? _unidadDeTrabajo.Aulas.ObtenerEntidad(a => a.IdAula == oferta.IdAula.Value).ValorRetorno
                        : null;

                    List<TDiaHorario>? horarios = null;
                    if (!string.IsNullOrEmpty(oferta.DiasHorarios))
                        horarios = JsonSerializer.Deserialize<List<TDiaHorario>>(oferta.DiasHorarios, _jsonOptions);

                    ofertasDisponibles.Add(new TOfertaDisponible
                    {
                        IdOferta = oferta.IdOferta,
                        Codigo = curso.Codigo,
                        Nombre = curso.Nombre,
                        Profesor = profesor != null ? $"{profesor.Nombre} {profesor.ApellidoPaterno}" : "",
                        Aula = aula?.NumeroAula,
                        Horario = horarios,
                        Creditos = curso.Creditos,
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

        public async Task<Respuesta<bool>> Matricular(int idEstudiante, int idOferta)
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

                var ahora = _tiempo.Ahora;
                if (ahora < periodo.FechaMatriculaInicio || ahora > periodo.FechaMatriculaFin)
                {
                    resultado.lpError("Error",
                        $"La matrícula estará disponible del {periodo.FechaMatriculaInicio:dd/MM/yyyy} al {periodo.FechaMatriculaFin:dd/MM/yyyy}");
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

                // Verificar que el curso pertenece a la carrera del estudiante
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                var perteneceACarrera = estudiante?.IdCarrera != null && _unidadDeTrabajo.CarreraCursos
                    .ObtenerEntidad(cc => cc.IdCarrera == estudiante.IdCarrera.Value && cc.IdCurso == oferta.IdCurso)
                    .ValorRetorno != null;

                if (!perteneceACarrera)
                {
                    resultado.lpError("Error", "Este curso no pertenece al plan de estudios de tu carrera");
                    return resultado;
                }

                // Verificar que el estudiante no está ya inscrito en este curso en el período actual
                var todasLasMatriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var idsOfertasPeriodoActual = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => o.IdPeriodo == periodo.IdPeriodo)
                    .ValorRetorno?.Select(o => o.IdOferta).ToHashSet() ?? new HashSet<int>();

                foreach (var mat in todasLasMatriculas.Where(m => idsOfertasPeriodoActual.Contains(m.IdOferta)))
                {
                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat?.IdCurso == oferta.IdCurso)
                    {
                        resultado.lpError("Error", "Ya estás matriculado en este curso en el período actual");
                        return resultado;
                    }
                }

                // Verificar choque de horario con cursos ya matriculados en el período actual
                if (!string.IsNullOrEmpty(oferta.DiasHorarios))
                {
                    var horariosNuevos = JsonSerializer.Deserialize<List<TDiaHorario>>(oferta.DiasHorarios, _jsonOptions)
                                         ?? new List<TDiaHorario>();

                    foreach (var mat in todasLasMatriculas.Where(m => idsOfertasPeriodoActual.Contains(m.IdOferta)))
                    {
                        var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                            .ValorRetorno;

                        if (ofertaMat == null || string.IsNullOrEmpty(ofertaMat.DiasHorarios)) continue;

                        var horariosExistentes = JsonSerializer.Deserialize<List<TDiaHorario>>(ofertaMat.DiasHorarios, _jsonOptions)
                                                 ?? new List<TDiaHorario>();

                        foreach (var slotNuevo in horariosNuevos)
                        {
                            foreach (var slotExistente in horariosExistentes)
                            {
                                if (!string.Equals(slotNuevo.Dia, slotExistente.Dia, StringComparison.OrdinalIgnoreCase))
                                    continue;

                                if (TimeSpan.TryParse(slotNuevo.HoraInicio, out var inicioNuevo) &&
                                    TimeSpan.TryParse(slotNuevo.HoraFin, out var finNuevo) &&
                                    TimeSpan.TryParse(slotExistente.HoraInicio, out var inicioExistente) &&
                                    TimeSpan.TryParse(slotExistente.HoraFin, out var finExistente))
                                {
                                    if (inicioNuevo < finExistente && finNuevo > inicioExistente)
                                    {
                                        var cursoCon = _unidadDeTrabajo.Cursos
                                            .ObtenerEntidad(c => c.IdCurso == ofertaMat.IdCurso)
                                            .ValorRetorno;
                                        resultado.lpError("Choque de horario",
                                            $"El horario choca con {cursoCon?.Nombre ?? "otro curso"} el {slotNuevo.Dia} de {slotExistente.HoraInicio} a {slotExistente.HoraFin}");
                                        return resultado;
                                    }
                                }
                            }
                        }
                    }
                }

                // Verificar que el estudiante no ya llevó este curso (pagado en período anterior)
                var matriculasPeriodosAnteriores = todasLasMatriculas
                    .Where(m => !idsOfertasPeriodoActual.Contains(m.IdOferta))
                    .ToList();

                foreach (var mat in matriculasPeriodosAnteriores)
                {
                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat?.IdCurso != oferta.IdCurso) continue;

                    var pagoAnterior = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula)
                        .ValorRetorno;
                    if (pagoAnterior?.Estado == "pagado")
                    {
                        resultado.lpError("Error", "Ya completaste este curso anteriormente");
                        return resultado;
                    }
                }

                // Verificar prerequisito
                var cursoSolicitado = _unidadDeTrabajo.Cursos
                    .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                    .ValorRetorno;

                if (cursoSolicitado?.idCursoRequisito.HasValue == true)
                {
                    var requisitoCompletado = matriculasPeriodosAnteriores.Any(mat =>
                    {
                        var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                            .ValorRetorno;
                        if (ofertaMat?.IdCurso != cursoSolicitado.idCursoRequisito.Value) return false;

                        var pago = _unidadDeTrabajo.Pagos
                            .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula)
                            .ValorRetorno;
                        return pago?.Estado == "pagado";
                    });

                    if (!requisitoCompletado)
                    {
                        resultado.lpError("Error", "No has completado el prerequisito requerido para este curso");
                        return resultado;
                    }
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

                _log.Registrar("matricula",
                    $"{estudiante?.Nombre} {estudiante?.ApellidoPaterno} se matriculó en {curso?.Nombre}",
                    "📝");

                // Enviar comprobante de matrícula por correo

                if (estudiante?.EmailPersonal != null)
                {
                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"] ?? "";

                    await _correo.EnviarComprobanteMatriculaAsync(
                        estudiante.EmailPersonal,
                        $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                        estudiante.carnet,
                        curso?.Codigo ?? "",
                        curso?.Nombre ?? "Curso",
                        periodo.Nombre,
                        matricula.FechaMatricula,
                        oferta.Precio,
                        nombreUniversidad,
                        correoRemitente
                    );
                }

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

        public async Task<Respuesta<bool>> RealizarPago(int idEstudiante, int idPago)
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

                var estudiantePago = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;
                _log.Registrar("pago",
                    $"{estudiantePago?.Nombre} {estudiantePago?.ApellidoPaterno} realizó el pago de {pago.Concepto}",
                    "💳");

                // Enviar comprobante de pago por correo
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                if (estudiante?.EmailPersonal != null)
                {
                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == matricula.IdOferta)
                        .ValorRetorno;

                    var periodo = oferta != null
                        ? _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo).ValorRetorno
                        : null;

                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"] ?? "";

                    await _correo.EnviarComprobantePagoAsync(
                        estudiante.EmailPersonal,
                        $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                        estudiante.carnet,
                        pago.IdPago,
                        pago.Concepto,
                        periodo?.Nombre ?? "",
                        pago.Monto,
                        pago.FechaPago!.Value,
                        nombreUniversidad,
                        correoRemitente
                    );
                }

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
