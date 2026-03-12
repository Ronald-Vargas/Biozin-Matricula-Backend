using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAjustesLN
    {

        Respuesta<int> Modificar(TAjustes ajustes);
        Respuesta<TAjustes> Obtener();
        Respuesta<IEnumerable<TAjustes>> Listar();

    }
}
