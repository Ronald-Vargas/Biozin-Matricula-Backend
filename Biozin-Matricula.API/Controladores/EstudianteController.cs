using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{

    [ApiController]
    [Route("api/[controller]")]
    public class EstudianteController : ControllerBase
    {

        private readonly IEstudianteLN _ln;
        public EstudianteController(IEstudianteLN ln) => _ln = ln;

        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TEstudiante obj) => Ok(_ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TEstudiante obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar")]
        public IActionResult Eliminar([FromBody] TEstudiante obj) => Ok(_ln.Eliminar(obj));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TEstudiante obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TEstudiante obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());

    }
}
