using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.InterfacesAD;

namespace Biozin_Matricula.AccesoDatos.Implementaciones
{
    public class UnidadTrabajoEF : IUnidadTrabajoEF
    {
        private readonly MatriculaDbContext _contexto;

        public IRepositorioAD<Curso> Cursos { get; private set; }
        public IRepositorioAD<Carrera> Carreras { get; private set; }
        public IRepositorioAD<Profesor> Profesores { get; private set; }
        public IRepositorioAD<Periodo> Periodos { get; private set; }
        public IRepositorioAD<Estudiante> Estudiantes { get; private set; }
        public IRepositorioAD<OfertaAcademica> OfertasAcademicas { get; private set; }
        public IRepositorioAD<CarreraCurso> CarreraCursos { get; private set; }
        public IRepositorioAD<Ajustes> Ajustes { get; private set; }

        public UnidadTrabajoEF(MatriculaDbContext contexto)
        {
            _contexto = contexto;
            Cursos = new RepositorioAD<Curso>(contexto);
            Carreras = new RepositorioAD<Carrera>(contexto);
            Profesores = new RepositorioAD<Profesor>(contexto);
            Periodos = new RepositorioAD<Periodo>(contexto);
            Estudiantes = new RepositorioAD<Estudiante>(contexto);
            OfertasAcademicas = new RepositorioAD<OfertaAcademica>(contexto);
            CarreraCursos = new RepositorioAD<CarreraCurso>(contexto);
            Ajustes = new RepositorioAD<Ajustes>(contexto);
        }

        public int Completar()
        {
            return _contexto.SaveChanges();
        }

        public void Dispose()
        {
            _contexto.Dispose();
        }
    }
}
