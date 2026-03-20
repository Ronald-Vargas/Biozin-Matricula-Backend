using Biozin_Matricula.AccesoDatos;
using Biozin_Matricula.AccesoDatos.Implementaciones;
using Biozin_Matricula.Dominio.DTO;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.LogicaNegocio.Implementaciones;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<MatriculaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionDB")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Unit of Work
builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();

// Business Logic
builder.Services.AddScoped<ICursoLN, CursoLN>();
builder.Services.AddScoped<ICarreraLN, CarreraLN>();
builder.Services.AddScoped<IProfesorLN, ProfesorLN>();
builder.Services.AddScoped<IPeriodoLN, PeriodoLN>();
builder.Services.AddScoped<IEstudianteLN, EstudianteLN>();
builder.Services.AddScoped<IOfertaAcademicaLN, OfertaAcademicaLN>();
builder.Services.AddScoped<ICarreraCursoLN, CarreraCursoLN>();
builder.Services.AddScoped<IAjustesLN, AjustesLN>();
builder.Services.AddScoped<ICorreoServicio, CorreoServicio>();
builder.Services.AddScoped<IAulaLN, AulaLN>();


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();
