using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{

    [ApiController]
    [Route("api/[controller]")]
    public class CarreraController : ControllerBase
    {

        private readonly ICarreraLN _ln;
        public CarreraController(ICarreraLN ln) => _ln = ln;

        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TCarrera obj) => Ok(_ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TCarrera obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar")]
        public IActionResult Eliminar([FromBody] TCarrera obj) => Ok(_ln.Eliminar(obj));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TCarrera obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TCarrera obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());

    }
}
