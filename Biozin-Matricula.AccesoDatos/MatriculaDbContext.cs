using Biozin_Matricula.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Biozin_Matricula.AccesoDatos
{
    public class MatriculaDbContext : DbContext
    {
        public MatriculaDbContext(DbContextOptions<MatriculaDbContext> options) : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<OfertaAcademica> OfertasAcademicas { get; set; }
        public DbSet<CarreraCurso> CarreraCursos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OfertaAcademica>()
                .HasOne(o => o.Periodo)
                .WithMany()
                .HasForeignKey(o => o.IdPeriodo);

            modelBuilder.Entity<OfertaAcademica>()
                .HasOne(o => o.Curso)
                .WithMany()
                .HasForeignKey(o => o.IdCurso);

            modelBuilder.Entity<OfertaAcademica>()
                .HasOne(o => o.Profesor)
                .WithMany()
                .HasForeignKey(o => o.IdProfesor);

            modelBuilder.Entity<CarreraCurso>()
                .HasOne(cc => cc.Carrera)
                .WithMany()
                .HasForeignKey(cc => cc.IdCarrera);

            modelBuilder.Entity<CarreraCurso>()
                .HasOne(cc => cc.Curso)
                .WithMany()
                .HasForeignKey(cc => cc.IdCurso);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Carrera)
                .WithMany()
                .HasForeignKey(e => e.IdCarrera);

            modelBuilder.Entity<OfertaAcademica>()
                .Property(o => o.Precio)
                .HasColumnType("decimal(18,2)");
        }
    }
}
