using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class EstudianteController : ControllerBase
    {

        private readonly IEstudianteLN _ln;
        private readonly IPortalEstudianteLN _portalLN;
        public EstudianteController(IEstudianteLN ln, IPortalEstudianteLN portalLN)
        {
            _ln = ln;
            _portalLN = portalLN;
        }

        [HttpPost("Insertar")]
        public async Task<IActionResult> Insertar([FromBody] TEstudiante obj) => Ok(await _ln.Insertar(obj));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TEstudiante obj) => Ok(_ln.Modificar(obj));

        [HttpDelete("Eliminar/{id}")]
        public IActionResult Eliminar(int id) => Ok(_ln.Eliminar(new TEstudiante { IdEstudiante = id }));

        [HttpPost("Obtener")]
        public IActionResult Obtener([FromBody] TEstudiante obj) => Ok(_ln.Obtener(obj));

        [HttpPost("Buscar")]
        public IActionResult Buscar([FromBody] TEstudiante obj) => Ok(_ln.Buscar(obj));

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());

        [HttpGet("MallaCurricular/{idEstudiante}")]
        public IActionResult ObtenerMalla(int idEstudiante, [FromQuery] int? idCarrera = null)
            => Ok(_portalLN.ObtenerMallaCurricular(idEstudiante, idCarrera));

        [HttpGet("Historial/{idEstudiante}")]
        public IActionResult ObtenerHistorial(int idEstudiante)
            => Ok(_portalLN.ObtenerHistorial(idEstudiante));

        [HttpPost("ReenviarCredenciales/{id}")]
        public async Task<IActionResult> ReenviarCredenciales(int id)
            => Ok(await _ln.ReenviarCredenciales(id));
    }
}
