using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.EntidadesTipadas
{
    public class TAdministrador
    {
        public int IdAdministrador { get; set; }

        public string Identificacion { get; set; }

        public string NombreCompleto { get; set; }

        public string? EmailInstitucional { get; set; }

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public string? Contraseña { get; set; }

        public bool Activo { get; set; } = true;

        public bool RequiereCambioContrasena { get; set; } = true;

    }
}
