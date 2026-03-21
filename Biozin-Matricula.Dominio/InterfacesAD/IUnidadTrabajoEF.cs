using Biozin_Matricula.Dominio.Entidades;

namespace Biozin_Matricula.Dominio.InterfacesAD
{
    public interface IUnidadTrabajoEF : IDisposable
    {
        IRepositorioAD<Curso> Cursos { get; }
        IRepositorioAD<Carrera> Carreras { get; }
        IRepositorioAD<Profesor> Profesores { get; }
        IRepositorioAD<Periodo> Periodos { get; }
        IRepositorioAD<Estudiante> Estudiantes { get; }
        IRepositorioAD<OfertaAcademica> OfertasAcademicas { get; }
        IRepositorioAD<CarreraCurso> CarreraCursos { get; }
        IRepositorioAD<Ajustes> Ajustes { get; }
        IRepositorioAD<Aula> Aulas { get; }
        IRepositorioAD<DiaHorario> DiasHorarios { get; }
        int Completar();
    }
}
