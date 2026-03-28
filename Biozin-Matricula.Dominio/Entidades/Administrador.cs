using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biozin_Matricula.Dominio.Entidades
{
    [Table("administradores")]
    public class Administrador
    {
        [Key]
        [Column("idAdministrador")]
        public int IdAdministrador { get; set; }

        [Column("identificacion")]
        public string Identificacion { get; set; }

        [Column("nombreCompleto")]
        public string NombreCompleto { get; set; }

        [Column("usuario")]
        public string Usuario { get; set; }

        [Column("correo")]
        public string Correo { get; set; }

        [Column("telefono")]
        public string Telefono { get; set; }

        [Column("contraseña")]
        public string Contraseña { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}

