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
        public DbSet<Ajustes> Ajustes { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Administrador> Administradores { get; set; }

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

            modelBuilder.Entity<OfertaAcademica>()
                .HasOne(o => o.Aula)
                .WithMany()
                .HasForeignKey(o => o.IdAula);

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

            // Matricula relationships
            modelBuilder.Entity<Matricula>()
                .HasOne(m => m.Estudiante)
                .WithMany()
                .HasForeignKey(m => m.IdEstudiante);

            modelBuilder.Entity<Matricula>()
                .HasOne(m => m.OfertaAcademica)
                .WithMany()
                .HasForeignKey(m => m.IdOferta);

            modelBuilder.Entity<Matricula>()
                .Property(m => m.Nota)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Matricula>()
                .HasIndex(m => new { m.IdEstudiante, m.IdOferta })
                .IsUnique()
                .HasDatabaseName("UQ_matricula");

            // Pago relationships
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Matricula)
                .WithMany()
                .HasForeignKey(p => p.IdMatricula);

            modelBuilder.Entity<Pago>()
                .Property(p => p.Monto)
                .HasColumnType("decimal(18,2)");

        }
    }
}
