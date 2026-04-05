using System.Security.Claims;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalEstudianteController : ControllerBase
    {
        private readonly IPortalEstudianteLN _ln;

        public PortalEstudianteController(IPortalEstudianteLN ln)
        {
            _ln = ln;
        }

















        [Authorize]
        [HttpGet("Perfil")]
        public IActionResult ObtenerPerfil()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPerfil(idEstudiante));
        }










        [Authorize]
        [HttpGet("Matricular/Ofertas")]
        public IActionResult ObtenerOfertasDisponibles()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerOfertasDisponibles(idEstudiante));
        }








        [Authorize]
        [HttpPost("Matricular")]
        public IActionResult Matricular([FromBody] TMatricularSolicitud solicitud)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.Matricular(idEstudiante, solicitud.IdOferta));
        }






        [Authorize]
        [HttpGet("Historial")]
        public IActionResult ObtenerHistorial()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerHistorial(idEstudiante));
        }







        [Authorize]
        [HttpGet("Pagos")]
        public IActionResult ObtenerPagos()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPagos(idEstudiante));
        }





        [Authorize]
        [HttpPost("Pagos/Pagar/{idPago}")]
        public IActionResult RealizarPago(int idPago)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.RealizarPago(idEstudiante, idPago));
        }






        private int ObtenerIdEstudiante()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }







    }
}
