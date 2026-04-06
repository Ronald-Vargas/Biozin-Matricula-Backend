using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{

    [ApiController]
    [Route("api/[controller]")]
    public class AdministradorController : ControllerBase
    {
        private readonly IAdministradorLN _ln;
        public AdministradorController(IAdministradorLN ln) => _ln = ln;


        [HttpPost("Insertar")]
        public async Task<IActionResult> Insertar([FromBody] TAdministrador obj) => Ok(await _ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TAdministrador obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar/{id}")]
        public IActionResult Eliminar(int id) => Ok(_ln.Eliminar(new TAdministrador { IdAdministrador = id }));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TAdministrador obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TAdministrador obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());
    }
}
