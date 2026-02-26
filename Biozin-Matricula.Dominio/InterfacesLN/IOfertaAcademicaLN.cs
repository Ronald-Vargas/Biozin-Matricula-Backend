using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IOfertaAcademicaLN
    {
        Respuesta<int> Insertar(TOfertaAcademica oferta);
        Respuesta<int> Modificar(TOfertaAcademica oferta);
        Respuesta<bool> Eliminar(TOfertaAcademica oferta);
        Respuesta<IEnumerable<TOfertaAcademica>> Obtener(TOfertaAcademica oferta);
        Respuesta<TOfertaAcademica> Buscar(TOfertaAcademica oferta);
        Respuesta<IEnumerable<TOfertaAcademica>> Listar();
    }
}
