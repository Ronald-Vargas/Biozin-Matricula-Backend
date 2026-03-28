using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Biozin_Matricula.API.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalEstudianteController : ControllerBase
    {
        private readonly IPortalEstudianteLN _ln;
        private readonly IConfiguration _config;

        public PortalEstudianteController(IPortalEstudianteLN ln, IConfiguration config)
        {
            _ln = ln;
            _config = config;
        }










        [HttpPost("Login")]
        public IActionResult Login([FromBody] TLoginEstudiante login)
        {
            var resultado = _ln.Login(login);

            if (resultado.blnError || resultado.ValorRetorno == null)
                return Ok(resultado);

            var token = GenerarToken(resultado.ValorRetorno);

            var respuesta = new Respuesta<TLoginRespuesta>
            {
                ValorRetorno = new TLoginRespuesta
                {
                    Token = token,
                    Perfil = resultado.ValorRetorno
                }
            };

            return Ok(respuesta);
        }







        [Authorize]
        [HttpGet("Perfil")]
        public IActionResult ObtenerPerfil()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPerfil(idEstudiante));
        }










        [Authorize]
        [HttpGet("Matricular/Ofertas")]
        public IActionResult ObtenerOfertasDisponibles()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerOfertasDisponibles(idEstudiante));
        }








        [Authorize]
        [HttpPost("Matricular")]
        public IActionResult Matricular([FromBody] TMatricularSolicitud solicitud)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.Matricular(idEstudiante, solicitud.IdOferta));
        }






        [Authorize]
        [HttpGet("Historial")]
        public IActionResult ObtenerHistorial()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerHistorial(idEstudiante));
        }







        [Authorize]
        [HttpGet("Pagos")]
        public IActionResult ObtenerPagos()
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.ObtenerPagos(idEstudiante));
        }





        [Authorize]
        [HttpPost("Pagos/Pagar/{idPago}")]
        public IActionResult RealizarPago(int idPago)
        {
            var idEstudiante = ObtenerIdEstudiante();
            if (idEstudiante == 0)
                return Unauthorized();

            return Ok(_ln.RealizarPago(idEstudiante, idPago));
        }






        private int ObtenerIdEstudiante()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }







        private string GenerarToken(TPerfilEstudiante perfil)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, perfil.IdEstudiante.ToString()),
                new Claim(ClaimTypes.Name, perfil.Nombre),
                new Claim(ClaimTypes.Email, perfil.EmailInstitucional ?? ""),
                new Claim("carnet", perfil.Carnet.ToString()),
                new Claim(ClaimTypes.Role, "Estudiante")
            };

            var expireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "480");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
