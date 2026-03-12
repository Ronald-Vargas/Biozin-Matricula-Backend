using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;


namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAjustesLN
    {

        Respuesta<int> Insertar(TAjustes ajustes);
        Respuesta<int> Modificar(TAjustes ajustes);
        Respuesta<TAjustes> Obtener();
        Respuesta<IEnumerable<TAjustes>> Listar();

    }
}
