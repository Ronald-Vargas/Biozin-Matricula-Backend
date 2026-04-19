using System.Security.Claims;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Estudiante")]
    [ApiController]
    [Route("api/[controller]")]
    public class PortalEstudianteController : ControllerBase
    {
        private readonly IPortalEstudianteLN _ln;

        public PortalEstudianteController(IPortalEstudianteLN ln)
        {
            _ln = ln;
        }

















        [HttpGet("Perfil")]
        public IActionResult ObtenerPerfil()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPerfil(idEstudiante));
        }










        [HttpGet("Matricular/Ofertas")]
        public IActionResult ObtenerOfertasDisponibles()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerOfertasDisponibles(idEstudiante));
        }








        [HttpPost("Matricular")]
        public async Task<IActionResult> Matricular([FromBody] TMatricularSolicitud solicitud)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(await _ln.Matricular(idEstudiante, solicitud.IdOferta));
        }

        [HttpPost("MatricularBulk")]
        public async Task<IActionResult> MatricularBulk([FromBody] TMatricularBulkSolicitud solicitud)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(await _ln.MatricularBulk(idEstudiante, solicitud));
        }






        [HttpGet("Historial")]
        public IActionResult ObtenerHistorial()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerHistorial(idEstudiante));
        }







        [HttpGet("Pagos")]
        public IActionResult ObtenerPagos()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPagos(idEstudiante));
        }





        [HttpPost("Pagos/Pagar/{idPago}")]
        public async Task<IActionResult> RealizarPago(int idPago)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(await _ln.RealizarPago(idEstudiante, idPago));
        }






        private int ObtenerIdEstudiante()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }







    }
}
