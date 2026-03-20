using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TAula
    {

        public int IdAula { get; set; }
        public string NumeroAula { get; set; }
        public int Capacidad { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool EsLaboratorio { get; set; } = true;

    }
}
