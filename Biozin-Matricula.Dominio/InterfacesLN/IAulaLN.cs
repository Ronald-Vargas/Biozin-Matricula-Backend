using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.InterfacesLN
{
    public interface IAulaLN
    {

        Respuesta<int> Insertar(TAula aula);
        Respuesta<int> Modificar(TAula aula);
        Respuesta<bool> Eliminar(TAula aula);
        Respuesta<IEnumerable<TAula>> Obtener(TAula aula);
        Respuesta<TAula> Buscar(TAula aula);
        Respuesta<IEnumerable<TAula>> Listar();
    }
}
