using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using System.Text.Json;

namespace Biozin_Matricula.Dominio.DTO
{
    public class AutoMapperProfile : Profile
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AutoMapperProfile()
        {
            CreateMap<Curso, TCurso>().ReverseMap();
            CreateMap<Carrera, TCarrera>().ReverseMap();
            CreateMap<Profesor, TProfesor>().ReverseMap();
            CreateMap<Periodo, TPeriodo>().ReverseMap();
            CreateMap<Estudiante, TEstudiante>().ReverseMap();
            CreateMap<CarreraCurso, TCarreraCurso>().ReverseMap();
            CreateMap<Ajustes, TAjustes>().ReverseMap();
            CreateMap<Aula, TAula>().ReverseMap();

            // OfertaAcademica -> TOfertaAcademica: deserialize JSON string to List
            CreateMap<OfertaAcademica, TOfertaAcademica>()
                .ForMember(dest => dest.DiasHorarios, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.DiasHorarios)
                        ? new List<TDiaHorario>()
                        : JsonSerializer.Deserialize<List<TDiaHorario>>(src.DiasHorarios, _jsonOptions)));

            // TOfertaAcademica -> OfertaAcademica: serialize List to JSON string, ignore nav properties
            CreateMap<TOfertaAcademica, OfertaAcademica>()
                .ForMember(dest => dest.DiasHorarios, opt => opt.MapFrom(src =>
                    src.DiasHorarios != null && src.DiasHorarios.Count > 0
                        ? JsonSerializer.Serialize(src.DiasHorarios, _jsonOptions)
                        : null))
                .ForMember(dest => dest.Periodo, opt => opt.Ignore())
                .ForMember(dest => dest.Curso, opt => opt.Ignore())
                .ForMember(dest => dest.Profesor, opt => opt.Ignore())
                .ForMember(dest => dest.Aula, opt => opt.Ignore());
        }
    }
}
