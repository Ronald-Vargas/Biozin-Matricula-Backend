using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class AjustesController : ControllerBase
    {

        private readonly IAjustesLN _ln;
        public AjustesController(IAjustesLN ln) => _ln = ln;


        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TAjustes obj) => Ok(_ln.Modificar(obj));

        [HttpGet("Obtener")]
        public IActionResult Obtener()
        {
            return Ok(_ln.Obtener());
        }

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());


        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TAjustes obj) => Ok(_ln.Insertar(obj));

    }
}

