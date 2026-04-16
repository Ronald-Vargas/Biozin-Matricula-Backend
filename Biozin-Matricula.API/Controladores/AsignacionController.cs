using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [Authorize(Roles = "Administrador")]
    [ApiController]
    [Route("api/[controller]")]
    public class AsignacionController : ControllerBase
    {
        private readonly ICarreraCursoLN _ln;
        public AsignacionController(ICarreraCursoLN ln) => _ln = ln;

        [HttpGet("Listar")]
        public IActionResult Listar() => Ok(_ln.Listar());

        [HttpPost("ObtenerPorCarrera")]
        public IActionResult ObtenerPorCarrera([FromBody] ObtenerPorCarreraRequest request)
            => Ok(_ln.Obtener(new TCarreraCurso { IdCarrera = request.IdCarrera }));

        [HttpPost("Insertar")]
        public IActionResult Insertar([FromBody] TCarreraCurso obj) => Ok(_ln.Insertar(obj));

        [HttpPost("InsertarMultiple")]
        public IActionResult InsertarMultiple([FromBody] InsertarMultipleRequest request)
            => Ok(_ln.InsertarMultiple(request.IdCarrera, request.Asignaciones));

        [HttpPut("Modificar")]
        public IActionResult Modificar([FromBody] TCarreraCurso obj) => Ok(_ln.Modificar(obj));

        [HttpPut("Eliminar")]
        public IActionResult Eliminar([FromBody] EliminarAsignacionRequest request)
            => Ok(_ln.Eliminar(new TCarreraCurso { Id = request.IdAsignacion }));

        [HttpPut("EliminarPorCarrera")]
        public IActionResult EliminarPorCarrera([FromBody] ObtenerPorCarreraRequest request)
            => Ok(_ln.EliminarPorCarrera(request.IdCarrera));
    }

    public class ObtenerPorCarreraRequest
    {
        public int IdCarrera { get; set; }
    }

    public class EliminarAsignacionRequest
    {
        public int IdAsignacion { get; set; }
    }

    public class InsertarMultipleRequest
    {
        public int IdCarrera { get; set; }
        public List<TCarreraCurso> Asignaciones { get; set; } = new();
    }
}
