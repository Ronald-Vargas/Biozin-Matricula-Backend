using AutoMapper;
using Biozin_Matricula.Dominio.Entidades;
using Biozin_Matricula.Dominio.EntidadesTipadas;

namespace Biozin_Matricula.Dominio.DTO
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Curso, TCurso>().ReverseMap();
            CreateMap<Carrera, TCarrera>().ReverseMap();
            CreateMap<Profesor, TProfesor>().ReverseMap();
            CreateMap<Periodo, TPeriodo>().ReverseMap();
            CreateMap<Estudiante, TEstudiante>().ReverseMap();
            CreateMap<OfertaAcademica, TOfertaAcademica>().ReverseMap();
            CreateMap<CarreraCurso, TCarreraCurso>().ReverseMap();
            CreateMap<Ajustes, TAjustes>().ReverseMap();
        }
    }
}
