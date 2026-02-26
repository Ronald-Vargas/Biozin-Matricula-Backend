using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{

    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase
    {

        private readonly ICursoLN _ln;
        public CursoController(ICursoLN ln) => _ln = ln;

        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TCurso obj) => Ok(_ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TCurso obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar")]
        public IActionResult Eliminar([FromBody] TCurso obj) => Ok(_ln.Eliminar(obj));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TCurso obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TCurso obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());

    }
}
