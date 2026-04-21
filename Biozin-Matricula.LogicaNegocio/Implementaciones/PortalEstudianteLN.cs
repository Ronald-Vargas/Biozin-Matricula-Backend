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

                var carreras = ObtenerCarrerasResumen(estudiante.IdEstudiante);

                resultado.ValorRetorno = new TPerfilEstudiante
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Nombre = estudiante.Nombre,
                    ApellidoPaterno = estudiante.ApellidoPaterno,
                    Carnet = estudiante.carnet,
                    Carreras = carreras,
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

                var carreras = ObtenerCarrerasResumen(idEstudiante);
                var idsCarreras = carreras.Select(c => c.IdCarrera).ToList();

                // Créditos totales: suma de todas las carreras del estudiante (sin duplicar cursos compartidos)
                int creditosTotales = 0;
                var idsCursosContados = new HashSet<int>();
                foreach (var idCarrera in idsCarreras)
                {
                    var cursosCarrera = _unidadDeTrabajo.CarreraCursos
                        .ObtenerEntidades(cc => cc.IdCarrera == idCarrera)
                        .ValorRetorno ?? Enumerable.Empty<CarreraCurso>();

                    foreach (var cc in cursosCarrera)
                    {
                        if (idsCursosContados.Contains(cc.IdCurso)) continue;
                        idsCursosContados.Add(cc.IdCurso);
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

                    if (periodoActual != null && oferta.IdPeriodo == periodoActual.IdPeriodo && mat.Estado == "en_curso")
                        creditosEnCurso += curso.Creditos;
                    else if (mat.Estado == "aprobado")
                        creditosAprobados += curso.Creditos;
                }

                // Semestre actual = cantidad de períodos distintos matriculados
                var periodosDistintos = todasLasMatriculas
                    .Select(m => _unidadDeTrabajo.OfertasAcademicas.ObtenerEntidad(o => o.IdOferta == m.IdOferta).ValorRetorno?.IdPeriodo)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .Count();

                resultado.ValorRetorno = new TPerfilEstudiante
                {
                    IdEstudiante = estudiante.IdEstudiante,
                    Nombre = estudiante.Nombre,
                    ApellidoPaterno = estudiante.ApellidoPaterno,
                    Carnet = estudiante.carnet,
                    Carreras = carreras,
                    SemestreActual = periodosDistintos > 0 ? periodosDistintos : estudiante.SemestreActual,
                    EmailInstitucional = estudiante.EmailInstitucional,
                    CreditosAprobados = creditosAprobados,
                    CreditosMatriculados = creditosMatriculados,
                    CreditosEnCurso = creditosEnCurso,
                    CreditosTotales = creditosTotales,
                    TipoBeca = estudiante.TipoBeca
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

                // Carreras del estudiante
                var relacionesCarrera = _unidadDeTrabajo.EstudianteCarreras
                    .ObtenerEntidades(ec => ec.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<EstudianteCarrera>();

                var idsCarreras = relacionesCarrera.Select(ec => ec.IdCarrera).ToList();

                if (idsCarreras.Count == 0)
                {
                    resultado.strMensajeRespuesta = "El estudiante no tiene una carrera asignada";
                    return resultado;
                }

                // Cursos que pertenecen al plan de CUALQUIERA de las carreras del estudiante
                var idsCursosCarrera = new HashSet<int>();
                foreach (var idCarrera in idsCarreras)
                {
                    var cursosDeCarrera = _unidadDeTrabajo.CarreraCursos
                        .ObtenerEntidades(cc => cc.IdCarrera == idCarrera)
                        .ValorRetorno ?? Enumerable.Empty<CarreraCurso>();
                    foreach (var cc in cursosDeCarrera)
                        idsCursosCarrera.Add(cc.IdCurso);
                }

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

                // Cursos aprobados en períodos anteriores (no reprobados)
                // → solo se excluyen si el pago fue realizado Y la matrícula fue aprobada
                var idsCursosPagados = new HashSet<int>();
                foreach (var mat in matriculasPeriodosAnteriores)
                {
                    if (mat.Estado != "aprobado") continue;

                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat == null) continue;

                    // Pago individual (flujo anterior)
                    var pagoIndividual = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula && p.Estado == "pagado")
                        .ValorRetorno;

                    // Pago bulk (flujo actual, IdMatricula == null, vinculado via pago_matriculas)
                    Pago? pagoBulk = null;
                    if (pagoIndividual == null)
                    {
                        var pm = _unidadDeTrabajo.PagoMatriculas
                            .ObtenerEntidad(x => x.IdMatricula == mat.IdMatricula)
                            .ValorRetorno;
                        if (pm != null)
                            pagoBulk = _unidadDeTrabajo.Pagos
                                .ObtenerEntidad(p => p.IdPago == pm.IdPago && p.Estado == "pagado")
                                .ValorRetorno;
                    }

                    if (pagoIndividual != null || pagoBulk != null)
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

                var ajustesOfertas = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();

                resultado.ValorRetorno = new TMatricularPeriodo
                {
                    IdPeriodo = periodo.IdPeriodo,
                    Nombre = periodo.Nombre,
                    FechaInicio = periodo.FechaInicio,
                    FechaFin = periodo.FechaFin,
                    FechaMatriculaFin = periodo.FechaMatriculaFin,
                    Ofertas = ofertasDisponibles,
                    MontoMatricula = ajustesOfertas?.montoMatricula ?? 100000m,
                    MontoInfraestructura = ajustesOfertas?.montoInfraestructura ?? 15000m
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

                // Verificar que el curso pertenece a ALGUNA de las carreras del estudiante
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                var idsCarrerasEstudiante = _unidadDeTrabajo.EstudianteCarreras
                    .ObtenerEntidades(ec => ec.IdEstudiante == idEstudiante)
                    .ValorRetorno?.Select(ec => ec.IdCarrera).ToList() ?? new List<int>();

                var perteneceACarrera = idsCarrerasEstudiante.Any(idCarrera =>
                    _unidadDeTrabajo.CarreraCursos
                        .ObtenerEntidad(cc => cc.IdCarrera == idCarrera && cc.IdCurso == oferta.IdCurso)
                        .ValorRetorno != null);

                if (!perteneceACarrera)
                {
                    resultado.lpError("Error", "Este curso no pertenece al plan de estudios de ninguna de tus carreras");
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

                // Verificar que el estudiante no ya aprobó este curso en un período anterior
                var matriculasPeriodosAnteriores = todasLasMatriculas
                    .Where(m => !idsOfertasPeriodoActual.Contains(m.IdOferta))
                    .ToList();

                foreach (var mat in matriculasPeriodosAnteriores)
                {
                    if (mat.Estado != "aprobado") continue;

                    var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                        .ValorRetorno;
                    if (ofertaMat?.IdCurso != oferta.IdCurso) continue;

                    var pagoIndividual = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula && p.Estado == "pagado")
                        .ValorRetorno;

                    Pago? pagoBulk = null;
                    if (pagoIndividual == null)
                    {
                        var pm = _unidadDeTrabajo.PagoMatriculas
                            .ObtenerEntidad(x => x.IdMatricula == mat.IdMatricula)
                            .ValorRetorno;
                        if (pm != null)
                            pagoBulk = _unidadDeTrabajo.Pagos
                                .ObtenerEntidad(p => p.IdPago == pm.IdPago && p.Estado == "pagado")
                                .ValorRetorno;
                    }

                    if (pagoIndividual != null || pagoBulk != null)
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
                    var urlCampus = ajustes?.sitioWeb ?? "";

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
                        correoRemitente,
                        urlCampus
                    );
                }

                ActualizarSemestreActual(idEstudiante);

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

        public async Task<Respuesta<List<TPagoEstudiante>>> ObtenerPagos(int idEstudiante)
        {
            var resultado = new Respuesta<List<TPagoEstudiante>>();
            try
            {
                var ahora = _tiempo.Ahora;

                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var idsMatriculas = matriculas.Select(m => m.IdMatricula).ToHashSet();
                var pagos = new List<TPagoEstudiante>();

                var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"] ?? "";

                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                // 1. Pagos individuales (flujo anterior): pago.IdMatricula != null
                foreach (var matricula in matriculas)
                {
                    var pagosMatricula = _unidadDeTrabajo.Pagos
                        .ObtenerEntidades(p => p.IdMatricula == matricula.IdMatricula)
                        .ValorRetorno ?? Enumerable.Empty<Pago>();

                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == matricula.IdOferta)
                        .ValorRetorno;

                    var periodo = oferta != null
                        ? _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo).ValorRetorno
                        : null;

                    foreach (var pago in pagosMatricula)
                    {
                        var estado = pago.Estado;
                        if (estado == "pendiente" && pago.FechaVencimiento < ahora)
                        {
                            estado = "vencido";
                            pago.Estado = "vencido";
                            _unidadDeTrabajo.Pagos.Modificar(pago);

                            matricula.Estado = "reprobado";
                            _unidadDeTrabajo.Matriculas.Modificar(matricula);
                            _unidadDeTrabajo.Completar();

                            _log.Registrar("pago_vencido",
                                $"Pago vencido para {estudiante?.Nombre} {estudiante?.ApellidoPaterno} — {pago.Concepto}",
                                "⛔");

                            if (estudiante?.EmailPersonal != null)
                            {
                                var curso = oferta != null
                                    ? _unidadDeTrabajo.Cursos.ObtenerEntidad(c => c.IdCurso == oferta.IdCurso).ValorRetorno
                                    : null;
                                try
                                {
                                    await _correo.EnviarNotificacionPagoVencidoAsync(
                                        estudiante.EmailPersonal,
                                        $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                                        estudiante.carnet,
                                        pago.Concepto,
                                        periodo?.Nombre ?? "",
                                        pago.Monto,
                                        pago.FechaVencimiento,
                                        curso != null ? new List<string> { curso.Nombre } : new List<string>(),
                                        nombreUniversidad,
                                        correoRemitente
                                    );
                                }
                                catch (Exception ex) { _logger.LogWarning("No se pudo enviar correo vencimiento: {0}", ex.Message); }
                            }
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

                // 2. Pagos bulk (nuevo flujo): pago.IdMatricula == null, vinculados via pago_matriculas
                var pagoMatriculas = _unidadDeTrabajo.PagoMatriculas
                    .ObtenerEntidades(pm => idsMatriculas.Contains(pm.IdMatricula))
                    .ValorRetorno ?? Enumerable.Empty<PagoMatricula>();

                var idsPagosBulkAgregados = new HashSet<int>();
                foreach (var pm in pagoMatriculas)
                {
                    if (idsPagosBulkAgregados.Contains(pm.IdPago)) continue;

                    var pago = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdPago == pm.IdPago && p.IdMatricula == null)
                        .ValorRetorno;

                    if (pago == null) continue;

                    idsPagosBulkAgregados.Add(pm.IdPago);

                    // Obtener todas las matrículas vinculadas a este pago bulk
                    var todasLasPmDeEstePago = _unidadDeTrabajo.PagoMatriculas
                        .ObtenerEntidades(x => x.IdPago == pago.IdPago)
                        .ValorRetorno ?? Enumerable.Empty<PagoMatricula>();

                    var matriculasBulk = todasLasPmDeEstePago
                        .Select(x => matriculas.FirstOrDefault(m => m.IdMatricula == x.IdMatricula))
                        .Where(m => m != null)
                        .ToList();

                    var mat = matriculasBulk.FirstOrDefault();
                    Periodo? periodo = null;
                    if (mat != null)
                    {
                        var oferta = _unidadDeTrabajo.OfertasAcademicas
                            .ObtenerEntidad(o => o.IdOferta == mat.IdOferta)
                            .ValorRetorno;
                        periodo = oferta != null
                            ? _unidadDeTrabajo.Periodos.ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo).ValorRetorno
                            : null;
                    }

                    var estado = pago.Estado;
                    if (estado == "pendiente" && pago.FechaVencimiento < ahora)
                    {
                        estado = "vencido";
                        pago.Estado = "vencido";
                        _unidadDeTrabajo.Pagos.Modificar(pago);

                        var nombresCursosBulk = new List<string>();
                        foreach (var matBulk in matriculasBulk)
                        {
                            if (matBulk == null) continue;
                            matBulk.Estado = "reprobado";
                            _unidadDeTrabajo.Matriculas.Modificar(matBulk);

                            var ofertaBulk = _unidadDeTrabajo.OfertasAcademicas
                                .ObtenerEntidad(o => o.IdOferta == matBulk.IdOferta).ValorRetorno;
                            if (ofertaBulk != null)
                            {
                                var cursoBulk = _unidadDeTrabajo.Cursos
                                    .ObtenerEntidad(c => c.IdCurso == ofertaBulk.IdCurso).ValorRetorno;
                                if (cursoBulk != null) nombresCursosBulk.Add(cursoBulk.Nombre);
                            }
                        }
                        _unidadDeTrabajo.Completar();

                        _log.Registrar("pago_vencido",
                            $"Pago vencido para {estudiante?.Nombre} {estudiante?.ApellidoPaterno} — {pago.Concepto}",
                            "⛔");

                        if (estudiante?.EmailPersonal != null)
                        {
                            try
                            {
                                await _correo.EnviarNotificacionPagoVencidoAsync(
                                    estudiante.EmailPersonal,
                                    $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                                    estudiante.carnet,
                                    pago.Concepto,
                                    periodo?.Nombre ?? "",
                                    pago.Monto,
                                    pago.FechaVencimiento,
                                    nombresCursosBulk,
                                    nombreUniversidad,
                                    correoRemitente
                                );
                            }
                            catch (Exception ex) { _logger.LogWarning("No se pudo enviar correo vencimiento: {0}", ex.Message); }
                        }
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

                // Verify ownership: individual pago (IdMatricula != null) o bulk (IdMatricula == null via pago_matriculas)
                Matricula? matricula;
                if (pago.IdMatricula != null)
                {
                    matricula = _unidadDeTrabajo.Matriculas
                        .ObtenerEntidad(m => m.IdMatricula == pago.IdMatricula && m.IdEstudiante == idEstudiante)
                        .ValorRetorno;
                }
                else
                {
                    var pm = _unidadDeTrabajo.PagoMatriculas
                        .ObtenerEntidad(pm => pm.IdPago == idPago)
                        .ValorRetorno;
                    matricula = pm != null
                        ? _unidadDeTrabajo.Matriculas
                            .ObtenerEntidad(m => m.IdMatricula == pm.IdMatricula && m.IdEstudiante == idEstudiante)
                            .ValorRetorno
                        : null;
                }

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

                if (pago.Estado == "vencido" || pago.FechaVencimiento < _tiempo.Ahora)
                {
                    resultado.lpError("Pago vencido", "El plazo de pago ha expirado. Los cursos asociados han sido marcados como reprobados.");
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
                    var urlCampus = ajustes?.sitioWeb ?? "";

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
                        correoRemitente,
                        urlCampus
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

        public async Task<Respuesta<bool>> MatricularBulk(int idEstudiante, TMatricularBulkSolicitud solicitud)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                if (solicitud.IdsOferta == null || solicitud.IdsOferta.Count == 0)
                {
                    resultado.lpError("Error", "Debe seleccionar al menos un curso");
                    return resultado;
                }

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

                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;

                var idsCarrerasEstudianteBulk = _unidadDeTrabajo.EstudianteCarreras
                    .ObtenerEntidades(ec => ec.IdEstudiante == idEstudiante)
                    .ValorRetorno?.Select(ec => ec.IdCarrera).ToList() ?? new List<int>();

                if (idsCarrerasEstudianteBulk.Count == 0)
                {
                    resultado.lpError("Error", "El estudiante no tiene una carrera asignada");
                    return resultado;
                }

                var todasLasMatriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var idsOfertasPeriodoActual = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => o.IdPeriodo == periodo.IdPeriodo)
                    .ValorRetorno?.Select(o => o.IdOferta).ToHashSet() ?? new HashSet<int>();

                // Cursos en los que el estudiante ya está inscrito en el período actual
                var idsCursosEnPeriodoActual = new HashSet<int>();
                foreach (var mat in todasLasMatriculas.Where(m => idsOfertasPeriodoActual.Contains(m.IdOferta)))
                {
                    var o = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(x => x.IdOferta == mat.IdOferta).ValorRetorno;
                    if (o != null) idsCursosEnPeriodoActual.Add(o.IdCurso);
                }

                // Cursos aprobados en períodos anteriores (reprobados pueden re-matricularse)
                var idsCursosPagados = new HashSet<int>();
                foreach (var mat in todasLasMatriculas.Where(m => !idsOfertasPeriodoActual.Contains(m.IdOferta)))
                {
                    if (mat.Estado != "aprobado") continue;

                    var o = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(x => x.IdOferta == mat.IdOferta).ValorRetorno;
                    if (o == null) continue;

                    // Pago individual
                    var pagoIndividual = _unidadDeTrabajo.Pagos
                        .ObtenerEntidad(p => p.IdMatricula == mat.IdMatricula && p.Estado == "pagado")
                        .ValorRetorno;

                    // Pago bulk
                    Pago? pagoBulk = null;
                    if (pagoIndividual == null)
                    {
                        var pm = _unidadDeTrabajo.PagoMatriculas
                            .ObtenerEntidad(x => x.IdMatricula == mat.IdMatricula)
                            .ValorRetorno;
                        if (pm != null)
                            pagoBulk = _unidadDeTrabajo.Pagos
                                .ObtenerEntidad(p => p.IdPago == pm.IdPago && p.Estado == "pagado")
                                .ValorRetorno;
                    }

                    if (pagoIndividual != null || pagoBulk != null)
                        idsCursosPagados.Add(o.IdCurso);
                }

                // Validar cada oferta seleccionada
                var ofertasValidadas = new List<(OfertaAcademica oferta, Curso curso)>();
                var horariosAcumulados = new List<(int idCurso, List<TDiaHorario> horarios)>();

                foreach (var idOferta in solicitud.IdsOferta)
                {
                    var oferta = _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == idOferta && o.Estado == true && o.IdPeriodo == periodo.IdPeriodo)
                        .ValorRetorno;

                    if (oferta == null)
                    {
                        resultado.lpError("Error", $"La oferta {idOferta} no está disponible");
                        return resultado;
                    }

                    if (oferta.Matriculados >= oferta.CupoMaximo)
                    {
                        resultado.lpError("Error", $"No hay cupos disponibles en una de las ofertas seleccionadas");
                        return resultado;
                    }

                    var perteneceACarrera = idsCarrerasEstudianteBulk.Any(idCarrera =>
                        _unidadDeTrabajo.CarreraCursos
                            .ObtenerEntidad(cc => cc.IdCarrera == idCarrera && cc.IdCurso == oferta.IdCurso)
                            .ValorRetorno != null);

                    if (!perteneceACarrera)
                    {
                        resultado.lpError("Error", "Un curso seleccionado no pertenece al plan de estudios de ninguna de tus carreras");
                        return resultado;
                    }

                    var curso = _unidadDeTrabajo.Cursos
                        .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                        .ValorRetorno;

                    if (curso == null)
                    {
                        resultado.lpError("Error", "No se encontró información del curso");
                        return resultado;
                    }

                    if (idsCursosEnPeriodoActual.Contains(curso.IdCurso))
                    {
                        resultado.lpError("Error", $"Ya estás matriculado en {curso.Nombre} en el período actual");
                        return resultado;
                    }

                    if (idsCursosPagados.Contains(curso.IdCurso))
                    {
                        resultado.lpError("Error", $"Ya completaste el curso {curso.Nombre} anteriormente");
                        return resultado;
                    }

                    if (curso.idCursoRequisito.HasValue && !idsCursosPagados.Contains(curso.idCursoRequisito.Value))
                    {
                        resultado.lpError("Error", $"No has completado el prerequisito requerido para {curso.Nombre}");
                        return resultado;
                    }

                    // Verificar choque de horario con matrículas existentes y con los otros cursos seleccionados
                    if (!string.IsNullOrEmpty(oferta.DiasHorarios))
                    {
                        var horariosNuevos = JsonSerializer.Deserialize<List<TDiaHorario>>(oferta.DiasHorarios, _jsonOptions)
                                             ?? new List<TDiaHorario>();

                        // vs existentes
                        foreach (var mat in todasLasMatriculas.Where(m => idsOfertasPeriodoActual.Contains(m.IdOferta)))
                        {
                            var ofertaMat = _unidadDeTrabajo.OfertasAcademicas
                                .ObtenerEntidad(o => o.IdOferta == mat.IdOferta).ValorRetorno;
                            if (ofertaMat == null || string.IsNullOrEmpty(ofertaMat.DiasHorarios)) continue;

                            var horariosEx = JsonSerializer.Deserialize<List<TDiaHorario>>(ofertaMat.DiasHorarios, _jsonOptions)
                                             ?? new List<TDiaHorario>();
                            if (HayChoqueHorario(horariosNuevos, horariosEx))
                            {
                                var cursoEx = _unidadDeTrabajo.Cursos
                                    .ObtenerEntidad(c => c.IdCurso == ofertaMat.IdCurso).ValorRetorno;
                                resultado.lpError("Choque de horario",
                                    $"El horario de {curso.Nombre} choca con {cursoEx?.Nombre ?? "otro curso"} ya matriculado");
                                return resultado;
                            }
                        }

                        // vs otros cursos en la misma selección bulk
                        foreach (var (_, horariosAcum) in horariosAcumulados)
                        {
                            if (HayChoqueHorario(horariosNuevos, horariosAcum))
                            {
                                resultado.lpError("Choque de horario",
                                    $"Hay un choque de horario entre los cursos seleccionados ({curso.Nombre})");
                                return resultado;
                            }
                        }

                        horariosAcumulados.Add((curso.IdCurso, horariosNuevos));
                    }

                    ofertasValidadas.Add((oferta, curso));
                    idsCursosEnPeriodoActual.Add(curso.IdCurso);
                }

                // Calcular descuento por beca
                decimal becaFactor = 0m;
                if (!string.IsNullOrEmpty(estudiante.TipoBeca) && estudiante.TipoBeca != "Ninguna")
                {
                    var cleaned = estudiante.TipoBeca.Replace("%", "").Trim();
                    if (decimal.TryParse(cleaned, out var pct))
                        becaFactor = pct / 100m;
                }

                var ajustesBulk = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                decimal montoMatriculaAjuste = ajustesBulk?.montoMatricula ?? 100000m;
                decimal montoInfraestructuraAjuste = ajustesBulk?.montoInfraestructura ?? 15000m;

                decimal subtotalCursos = ofertasValidadas.Sum(x => x.oferta.Precio * (1 - becaFactor));
                decimal montoTotal = subtotalCursos + montoMatriculaAjuste + montoInfraestructuraAjuste;

                // Crear matrículas
                var matriculasCreadas = new List<Matricula>();
                foreach (var (oferta, _) in ofertasValidadas)
                {
                    var matricula = new Matricula
                    {
                        IdEstudiante = idEstudiante,
                        IdOferta = oferta.IdOferta,
                        FechaMatricula = DateTime.UtcNow,
                        Nota = null,
                        Estado = "en_curso"
                    };
                    _unidadDeTrabajo.Matriculas.Insertar(matricula);
                    oferta.Matriculados++;
                    _unidadDeTrabajo.OfertasAcademicas.Modificar(oferta);
                    matriculasCreadas.Add(matricula);
                }
                _unidadDeTrabajo.Completar();

                // Crear un único pago agrupado
                int cantidadCursos = ofertasValidadas.Count;
                var pago = new Pago
                {
                    IdMatricula = null,
                    Concepto = $"Matrícula {periodo.Nombre} - {cantidadCursos} curso(s)",
                    Monto = montoTotal,
                    FechaVencimiento = periodo.FechaMatriculaFin.AddDays(30),
                    FechaPago = solicitud.Financiar ? null : DateTime.UtcNow,
                    Estado = solicitud.Financiar ? "pendiente" : "pagado"
                };
                _unidadDeTrabajo.Pagos.Insertar(pago);
                _unidadDeTrabajo.Completar();

                // Crear registros de detalle pago_matriculas
                foreach (var matricula in matriculasCreadas)
                {
                    _unidadDeTrabajo.PagoMatriculas.Insertar(new PagoMatricula
                    {
                        IdPago = pago.IdPago,
                        IdMatricula = matricula.IdMatricula
                    });
                }
                _unidadDeTrabajo.Completar();

                var nombresCursos = string.Join(", ", ofertasValidadas.Select(x => x.curso.Nombre));
                _log.Registrar("matricula",
                    $"{estudiante.Nombre} {estudiante.ApellidoPaterno} se matriculó en {cantidadCursos} curso(s): {nombresCursos}",
                    "📝");

                // Enviar comprobante de matrícula bulk por correo
                if (!string.IsNullOrEmpty(estudiante.EmailPersonal))
                {
                    var ajustes = _unidadDeTrabajo.Ajustes.Listar().ValorRetorno?.FirstOrDefault();
                    var nombreUniversidad = ajustes?.nombreUniversidad ?? "Universidad";
                    var correoRemitente = ajustes?.correoInstitucional ?? _config["Mail:Remitente"] ?? "";

                    var cursosList = ofertasValidadas.Select(x =>
                        (x.curso.Codigo, x.curso.Nombre, x.oferta.Precio * (1 - becaFactor))
                    ).ToList();

                    await _correo.EnviarComprobanteMatriculaBulkAsync(
                        estudiante.EmailPersonal,
                        $"{estudiante.Nombre} {estudiante.ApellidoPaterno}",
                        estudiante.carnet,
                        cursosList,
                        periodo.Nombre,
                        DateTime.UtcNow,
                        montoMatriculaAjuste,
                        montoInfraestructuraAjuste,
                        montoTotal,
                        solicitud.Financiar,
                        nombreUniversidad,
                        correoRemitente
                    );
                }

                ActualizarSemestreActual(idEstudiante);

                resultado.ValorRetorno = true;
                resultado.strMensajeRespuesta = solicitud.Financiar
                    ? $"Matrícula registrada. El pago de ₡{montoTotal:N0} quedó pendiente."
                    : $"Matrícula y pago de ₡{montoTotal:N0} realizados exitosamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error MatricularBulk: {0}", ex.Message);
                resultado.lpError("Error al procesar la matrícula", ex.Message);
            }
            return resultado;
        }

        public Respuesta<TMallaCurricular> ObtenerMallaCurricular(int idEstudiante, int? idCarrera = null)
        {
            var resultado = new Respuesta<TMallaCurricular>();
            try
            {
                var relaciones = _unidadDeTrabajo.EstudianteCarreras
                    .ObtenerEntidades(ec => ec.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<EstudianteCarrera>();

                var idsCarreras = relaciones.Select(ec => ec.IdCarrera).ToList();
                if (idsCarreras.Count == 0)
                {
                    resultado.lpError("Sin carrera", "El estudiante no tiene una carrera asignada.");
                    return resultado;
                }

                // Si se pasa idCarrera, validar que pertenezca al estudiante; si no, usar la primera
                int idCarreraEfectiva = idCarrera.HasValue && idsCarreras.Contains(idCarrera.Value)
                    ? idCarrera.Value
                    : idsCarreras[0];

                var carrera = _unidadDeTrabajo.Carreras.ObtenerEntidad(c => c.IdCarrera == idCarreraEfectiva).ValorRetorno;
                if (carrera == null)
                {
                    resultado.lpError("No encontrado", "No se encontró la carrera del estudiante.");
                    return resultado;
                }

                var carreraCursos = _unidadDeTrabajo.CarreraCursos
                    .ObtenerEntidades(cc => cc.IdCarrera == carrera.IdCarrera)
                    .ValorRetorno ?? [];

                var cursos = _unidadDeTrabajo.Cursos.Listar().ValorRetorno ?? [];
                var cursosMap = cursos.ToDictionary(c => c.IdCurso);

                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? [];

                var ofertaIds = matriculas.Select(m => m.IdOferta).ToHashSet();
                var todasOfertas = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => ofertaIds.Contains(o.IdOferta))
                    .ValorRetorno ?? [];
                var ofertasMap = todasOfertas.ToDictionary(o => o.IdOferta);

                // Build lookup: idCurso -> matrícula (prefer aprobado > en_curso > reprobado)
                var matriculasPorCurso = new Dictionary<int, Dominio.Entidades.Matricula>();
                foreach (var m in matriculas)
                {
                    if (!ofertasMap.TryGetValue(m.IdOferta, out var oferta)) continue;
                    if (!matriculasPorCurso.ContainsKey(oferta.IdCurso))
                        matriculasPorCurso[oferta.IdCurso] = m;
                    else
                    {
                        var existing = matriculasPorCurso[oferta.IdCurso];
                        if (m.Estado == "aprobado" || (m.Estado == "en_curso" && existing.Estado != "aprobado"))
                            matriculasPorCurso[oferta.IdCurso] = m;
                    }
                }

                var aprobados = matriculasPorCurso.Where(kv => kv.Value.Estado == "aprobado").Select(kv => kv.Key).ToHashSet();

                var semestresAgrupados = carreraCursos
                    .GroupBy(cc => cc.Semestre)
                    .OrderBy(g => g.Key);

                int totalCreditos = 0, creditosAprobados = 0, creditosEnCurso = 0;
                var semestres = new List<TSemestreMalla>();

                foreach (var grupo in semestresAgrupados)
                {
                    var cursosSemestre = new List<TCursoMalla>();
                    foreach (var cc in grupo.OrderBy(cc => cc.IdCurso))
                    {
                        if (!cursosMap.TryGetValue(cc.IdCurso, out var curso)) continue;
                        totalCreditos += curso.Creditos;

                        string estado;
                        decimal? nota = null;

                        if (matriculasPorCurso.TryGetValue(cc.IdCurso, out var mat))
                        {
                            estado = mat.Estado; // aprobado | en_curso | reprobado
                            nota = mat.Nota;
                            if (estado == "aprobado") creditosAprobados += curso.Creditos;
                            else if (estado == "en_curso") creditosEnCurso += curso.Creditos;
                        }
                        else
                        {
                            // Not yet taken — check if prerequisite is met
                            bool requisitoOk = !curso.idCursoRequisito.HasValue || aprobados.Contains(curso.idCursoRequisito.Value);
                            estado = requisitoOk ? "disponible" : "pendiente";
                        }

                        string? nombreRequisito = null;
                        if (curso.idCursoRequisito.HasValue && cursosMap.TryGetValue(curso.idCursoRequisito.Value, out var cursoReq))
                            nombreRequisito = cursoReq.Nombre;

                        cursosSemestre.Add(new TCursoMalla
                        {
                            IdCurso = cc.IdCurso,
                            Codigo = curso.Codigo,
                            Nombre = curso.Nombre,
                            Creditos = curso.Creditos,
                            Semestre = cc.Semestre,
                            EsVirtual = curso.EsVirtual,
                            IdCursoRequisito = curso.idCursoRequisito,
                            NombreRequisito = nombreRequisito,
                            Estado = estado,
                            Nota = nota
                        });
                    }
                    semestres.Add(new TSemestreMalla { Numero = grupo.Key, Cursos = cursosSemestre });
                }

                resultado.ValorRetorno = new TMallaCurricular
                {
                    NombreCarrera = carrera.Nombre,
                    TotalCreditos = totalCreditos,
                    CreditosAprobados = creditosAprobados,
                    CreditosEnCurso = creditosEnCurso,
                    Semestres = semestres
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerMallaCurricular: {0}", ex.Message);
                resultado.lpError("Error", ex.Message);
            }
            return resultado;
        }

        private List<TCarreraResumen> ObtenerCarrerasResumen(int idEstudiante)
        {
            var relaciones = _unidadDeTrabajo.EstudianteCarreras
                .ObtenerEntidades(ec => ec.IdEstudiante == idEstudiante)
                .ValorRetorno ?? Enumerable.Empty<EstudianteCarrera>();

            var lista = new List<TCarreraResumen>();
            foreach (var rel in relaciones)
            {
                var carrera = _unidadDeTrabajo.Carreras.ObtenerEntidad(c => c.IdCarrera == rel.IdCarrera).ValorRetorno;
                if (carrera != null)
                    lista.Add(new TCarreraResumen { IdCarrera = carrera.IdCarrera, Codigo = carrera.Codigo, Nombre = carrera.Nombre });
            }
            return lista;
        }

        private static bool HayChoqueHorario(List<TDiaHorario> a, List<TDiaHorario> b)
        {
            foreach (var slotA in a)
            {
                foreach (var slotB in b)
                {
                    if (!string.Equals(slotA.Dia, slotB.Dia, StringComparison.OrdinalIgnoreCase)) continue;
                    if (TimeSpan.TryParse(slotA.HoraInicio, out var iniA) &&
                        TimeSpan.TryParse(slotA.HoraFin, out var finA) &&
                        TimeSpan.TryParse(slotB.HoraInicio, out var iniB) &&
                        TimeSpan.TryParse(slotB.HoraFin, out var finB))
                    {
                        if (iniA < finB && finA > iniB) return true;
                    }
                }
            }
            return false;
        }

        private void ActualizarSemestreActual(int idEstudiante)
        {
            try
            {
                var estudiante = _unidadDeTrabajo.Estudiantes
                    .ObtenerEntidad(e => e.IdEstudiante == idEstudiante)
                    .ValorRetorno;
                if (estudiante == null) return;

                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdEstudiante == idEstudiante)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var periodosDistintos = matriculas
                    .Select(m => _unidadDeTrabajo.OfertasAcademicas
                        .ObtenerEntidad(o => o.IdOferta == m.IdOferta).ValorRetorno?.IdPeriodo)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .Count();

                if (periodosDistintos > 0 && estudiante.SemestreActual != periodosDistintos)
                {
                    estudiante.SemestreActual = periodosDistintos;
                    _unidadDeTrabajo.Estudiantes.Modificar(estudiante);
                    _unidadDeTrabajo.Completar();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("No se pudo actualizar semestre_actual: {0}", ex.Message);
            }
        }
    }
}
