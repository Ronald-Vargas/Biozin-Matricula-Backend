using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class FinanzasController : ControllerBase
    {
        private readonly IFinanzasLN _ln;

        public FinanzasController(IFinanzasLN ln)
        {
            _ln = ln;
        }

        [HttpGet("ResumenPeriodoActual")]
        public IActionResult ResumenPeriodoActual() => Ok(_ln.ObtenerResumenPeriodoActual());

        [HttpGet("ResumenPorPeriodo/{idPeriodo}")]
        public IActionResult ResumenPorPeriodo(int idPeriodo) => Ok(_ln.ObtenerResumenPorPeriodo(idPeriodo));

        [HttpGet("DetallesPorPeriodo/{idPeriodo}")]
        public IActionResult DetallesPorPeriodo(int idPeriodo) => Ok(_ln.ObtenerDetallesPorPeriodo(idPeriodo));

        [HttpGet("ResumenTodosPeriodos")]
        public IActionResult ResumenTodosPeriodos() => Ok(_ln.ObtenerResumenTodosPeriodos());
    }
}
