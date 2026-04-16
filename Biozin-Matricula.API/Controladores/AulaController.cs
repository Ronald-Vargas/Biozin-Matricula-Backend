using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.LogicaNegocio.Implementaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class AulaController : ControllerBase
    {

        private readonly IAulaLN _ln;
        public AulaController(IAulaLN ln) => _ln = ln;


        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TAula obj) => Ok(_ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TAula obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar/{id}")]
        public IActionResult Eliminar(int id) => Ok(_ln.Eliminar(new TAula { IdAula = id }));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TAula obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TAula obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());
   
    }
}
