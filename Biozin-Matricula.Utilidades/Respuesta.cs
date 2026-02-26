using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Utilidades
{
    public class Respuesta
    {

        public T ValorRetorno { get; set; }
        public string strMensajeRespuesta { get; set; } = string.Empty;
        public bool blnError { get; set; } = false;
        public string strTituloRespuesta { get; set; } = string.Empty;

        public void lpError(string titulo, string detalle)
        {
            blnError = true;
            strTituloRespuesta = titulo;
            strMensajeRespuesta = detalle;
        }

    }
}
