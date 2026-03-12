using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TAjustes
    {

        public int idAjuste { get; set; }

        public string nombreUniversidad { get; set; } = string.Empty;

        public string? sitioWeb { get; set; }

        public string? correoInstitucional { get; set; }

        public string? telefono { get; set; }

        public string? direccion { get; set; }

        public string? provincia { get; set; }

        public string? canton { get; set; }

        public string? distrito { get; set; }
    }
}
