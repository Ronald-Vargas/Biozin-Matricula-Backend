  using Biozin_Matricula.AccesoDatos;
using Biozin_Matricula.AccesoDatos.Implementaciones;
using Biozin_Matricula.Dominio.DTO;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.InterfacesAD;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.LogicaNegocio.Implementaciones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                .ToList();
            var mensaje = errores.Count > 0 ? string.Join(" | ", errores) : "El cuerpo de la solicitud contiene datos inválidos o mal formados.";
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
            {
                blnError = true,
                strTituloRespuesta = "Solicitud inválida",
                strMensajeRespuesta = mensaje
            });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<MatriculaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionDB")));

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

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
builder.Services.AddScoped<IPortalEstudianteLN, PortalEstudianteLN>();
builder.Services.AddScoped<IPortalProfesorLN, PortalProfesorLN>();
builder.Services.AddScoped<IAdministradorLN, AdministradorLN>();
builder.Services.AddScoped<ILogActividadServicio, LogActividadServicio>();
builder.Services.AddScoped<IFinanzasLN, FinanzasLN>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProviderConfigurable>();
else
    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
