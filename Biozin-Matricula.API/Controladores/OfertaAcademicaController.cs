using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class OfertaAcademicaController : ControllerBase
    {
        private readonly IOfertaAcademicaLN _ln;
        public OfertaAcademicaController(IOfertaAcademicaLN ln) => _ln = ln;

        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TOfertaAcademica obj) => Ok(_ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TOfertaAcademica obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar/{id}")]
        public IActionResult Eliminar(int id) => Ok(_ln.Eliminar(new TOfertaAcademica { IdOferta = id }));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TOfertaAcademica obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TOfertaAcademica obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());
    }
}
