using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Microsoft.AspNetCore.Mvc;

namespace Biozin_Matricula.API.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLN _ln;
        public AuthController(IAuthLN ln) => _ln = ln;

        [HttpPost("Login")]
        public IActionResult Login([FromBody] TLogin obj) => Ok(_ln.Login(obj));

        [HttpPost("CambiarContrasenaTemporaria")]
        public IActionResult CambiarContrasenaTemporaria([FromBody] TCambioContrasena obj) => Ok(_ln.CambiarContrasenaTemporaria(obj));
    }
}
