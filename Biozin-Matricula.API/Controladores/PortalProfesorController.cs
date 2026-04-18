using System.Security.Claims;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Profesor")]
    [ApiController]
    [Route("api/[controller]")]
    public class PortalProfesorController : ControllerBase
    {
        private readonly IPortalProfesorLN _ln;

        public PortalProfesorController(IPortalProfesorLN ln)
        {
            _ln = ln;
        }

        [HttpGet("Perfil")]
        public IActionResult ObtenerPerfil()
        {
            var idProfesor = ObtenerIdProfesor();
            if (idProfesor == 0) return Unauthorized();

            return Ok(_ln.ObtenerPerfil(idProfesor));
        }

        [HttpGet("MisCursos")]
        public IActionResult ObtenerMisCursos()
        {
            var idProfesor = ObtenerIdProfesor();
            if (idProfesor == 0) return Unauthorized();

            return Ok(_ln.ObtenerMisCursos(idProfesor));
        }

        [HttpGet("Cursos/{idOferta}/Estudiantes")]
        public IActionResult ObtenerEstudiantesCurso(int idOferta)
        {
            var idProfesor = ObtenerIdProfesor();
            if (idProfesor == 0) return Unauthorized();

            return Ok(_ln.ObtenerEstudiantesCurso(idProfesor, idOferta));
        }

        [HttpPut("AsignarNota")]
        public IActionResult AsignarNota([FromBody] TAsignarNota solicitud)
        {
            var idProfesor = ObtenerIdProfesor();
            if (idProfesor == 0) return Unauthorized();

            return Ok(_ln.AsignarNota(idProfesor, solicitud));
        }

        private int ObtenerIdProfesor()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
