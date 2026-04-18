using System.Text.Json;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.Extensions.Logging;

namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class PortalProfesorLN : IPortalProfesorLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<PortalProfesorLN> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public PortalProfesorLN(IUnidadTrabajoEF unidadDeTrabajo, ILogger<PortalProfesorLN> logger)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
            _logger = logger;
        }

        public Respuesta<TPerfilProfesor> ObtenerPerfil(int idProfesor)
        {
            var resultado = new Respuesta<TPerfilProfesor>();
            try
            {
                var profesor = _unidadDeTrabajo.Profesores
                    .ObtenerEntidad(p => p.IdProfesor == idProfesor)
                    .ValorRetorno;

                if (profesor == null)
                {
                    resultado.lpError("Error", "Profesor no encontrado");
                    return resultado;
                }

                resultado.ValorRetorno = new TPerfilProfesor
                {
                    IdProfesor = profesor.IdProfesor,
                    Nombre = profesor.Nombre,
                    ApellidoPaterno = profesor.ApellidoPaterno,
                    ApellidoMaterno = profesor.ApellidoMaterno,
                    EmailInstitucional = profesor.EmailInstitucional ?? string.Empty,
                    Titulo = profesor.Titulo,
                    Especialidad = profesor.Especialidad,
                    CursosAsignados = profesor.CursosAsignados ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerPerfil Profesor: {0}", ex.Message);
                resultado.lpError("Error al obtener perfil", ex.Message);
            }
            return resultado;
        }

        public Respuesta<List<TOfertaProfesor>> ObtenerMisCursos(int idProfesor)
        {
            var resultado = new Respuesta<List<TOfertaProfesor>>();
            try
            {
                var ofertas = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidades(o => o.IdProfesor == idProfesor)
                    .ValorRetorno ?? Enumerable.Empty<OfertaAcademica>();

                var lista = new List<TOfertaProfesor>();

                foreach (var oferta in ofertas)
                {
                    var curso = _unidadDeTrabajo.Cursos
                        .ObtenerEntidad(c => c.IdCurso == oferta.IdCurso)
                        .ValorRetorno;

                    var periodo = _unidadDeTrabajo.Periodos
                        .ObtenerEntidad(p => p.IdPeriodo == oferta.IdPeriodo)
                        .ValorRetorno;

                    var aula = oferta.IdAula.HasValue
                        ? _unidadDeTrabajo.Aulas.ObtenerEntidad(a => a.IdAula == oferta.IdAula.Value).ValorRetorno
                        : null;

                    lista.Add(new TOfertaProfesor
                    {
                        IdOferta = oferta.IdOferta,
                        CodigoCurso = curso?.Codigo ?? string.Empty,
                        NombreCurso = curso?.Nombre ?? string.Empty,
                        Creditos = curso?.Creditos ?? 0,
                        NombrePeriodo = periodo?.Nombre ?? string.Empty,
                        NombreAula = aula != null ? $"{aula.NumeroAula} - {aula.Descripcion}".TrimEnd('-', ' ') : null,
                        Horario = FormatearHorario(oferta.DiasHorarios),
                        CupoMaximo = oferta.CupoMaximo,
                        Matriculados = oferta.Matriculados,
                        Estado = oferta.Estado
                    });
                }

                resultado.ValorRetorno = lista;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerMisCursos: {0}", ex.Message);
                resultado.lpError("Error al obtener cursos", ex.Message);
            }
            return resultado;
        }

        public Respuesta<List<TEstudianteEnCurso>> ObtenerEstudiantesCurso(int idProfesor, int idOferta)
        {
            var resultado = new Respuesta<List<TEstudianteEnCurso>>();
            try
            {
                var oferta = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidad(o => o.IdOferta == idOferta)
                    .ValorRetorno;

                if (oferta == null || oferta.IdProfesor != idProfesor)
                {
                    resultado.lpError("Acceso denegado", "No tiene permiso para ver este curso");
                    return resultado;
                }

                var matriculas = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidades(m => m.IdOferta == idOferta)
                    .ValorRetorno ?? Enumerable.Empty<Matricula>();

                var lista = new List<TEstudianteEnCurso>();

                foreach (var matricula in matriculas)
                {
                    var estudiante = _unidadDeTrabajo.Estudiantes
                        .ObtenerEntidad(e => e.IdEstudiante == matricula.IdEstudiante)
                        .ValorRetorno;

                    if (estudiante == null) continue;

                    lista.Add(new TEstudianteEnCurso
                    {
                        IdMatricula = matricula.IdMatricula,
                        IdEstudiante = estudiante.IdEstudiante,
                        Carnet = (long)estudiante.carnet,
                        Nombre = estudiante.Nombre,
                        ApellidoPaterno = estudiante.ApellidoPaterno,
                        ApellidoMaterno = estudiante.ApellidoMaterno,
                        EmailInstitucional = estudiante.EmailInstitucional ?? string.Empty,
                        Nota = matricula.Nota,
                        Estado = matricula.Estado
                    });
                }

                resultado.ValorRetorno = lista.OrderBy(e => e.ApellidoPaterno).ThenBy(e => e.Nombre).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ObtenerEstudiantesCurso: {0}", ex.Message);
                resultado.lpError("Error al obtener estudiantes", ex.Message);
            }
            return resultado;
        }

        public Respuesta<bool> AsignarNota(int idProfesor, TAsignarNota solicitud)
        {
            var resultado = new Respuesta<bool>();
            try
            {
                var matricula = _unidadDeTrabajo.Matriculas
                    .ObtenerEntidad(m => m.IdMatricula == solicitud.IdMatricula)
                    .ValorRetorno;

                if (matricula == null)
                {
                    resultado.lpError("Error", "Matrícula no encontrada");
                    return resultado;
                }

                // Verificar que la oferta pertenece al profesor
                var oferta = _unidadDeTrabajo.OfertasAcademicas
                    .ObtenerEntidad(o => o.IdOferta == matricula.IdOferta)
                    .ValorRetorno;

                if (oferta == null || oferta.IdProfesor != idProfesor)
                {
                    resultado.lpError("Acceso denegado", "No tiene permiso para calificar este curso");
                    return resultado;
                }

                if (solicitud.Nota < 0 || solicitud.Nota > 100)
                {
                    resultado.lpError("Error", "La nota debe estar entre 0 y 100");
                    return resultado;
                }

                matricula.Nota = solicitud.Nota;
                matricula.Estado = solicitud.Nota >= 70 ? "aprobado" : "reprobado";

                _unidadDeTrabajo.Matriculas.Modificar(matricula);
                _unidadDeTrabajo.Completar();

                resultado.ValorRetorno = true;
                resultado.strTituloRespuesta = "Éxito";
                resultado.strMensajeRespuesta = "Nota asignada correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error AsignarNota: {0}", ex.Message);
                resultado.lpError("Error al asignar nota", ex.Message);
            }
            return resultado;
        }

        private static string? FormatearHorario(string? diasHorariosJson)
        {
            if (string.IsNullOrWhiteSpace(diasHorariosJson)) return null;
            try
            {
                var slots = JsonSerializer.Deserialize<List<HorarioSlot>>(diasHorariosJson, _jsonOptions);
                if (slots == null || slots.Count == 0) return null;
                return string.Join(", ", slots.Select(s => $"{s.Dia} {s.HoraInicio}-{s.HoraFin}"));
            }
            catch
            {
                return diasHorariosJson;
            }
        }

        private class HorarioSlot
        {
            public string Dia { get; set; } = string.Empty;
            public string HoraInicio { get; set; } = string.Empty;
            public string HoraFin { get; set; } = string.Empty;
        }
    }
}
