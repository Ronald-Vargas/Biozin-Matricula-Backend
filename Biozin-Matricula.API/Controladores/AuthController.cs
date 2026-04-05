using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Biozin_Matricula.Dominio.EntidadesTipadas;
using Biozin_Matricula.Dominio.InterfacesLN;
using Biozin_Matricula.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Biozin_Matricula.API.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IPortalEstudianteLN _portalEstudianteLN;
        private readonly IAdministradorLN _administradorLN;
        private readonly IConfiguration _config;

        public AuthController(IPortalEstudianteLN portalEstudianteLN, IAdministradorLN administradorLN, IConfiguration config)
        {
            _portalEstudianteLN = portalEstudianteLN;
            _administradorLN = administradorLN;
            _config = config;
        }

        /// <summary>
        /// Login unificado. Detecta el rol según el dominio del email:
        ///   @est.biozin.edu.cr   → Estudiante
        ///   @admin.biozin.edu.cr → Administrador
        /// </summary>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] TLoginRequest login)
        {
            var dominio = ObtenerDominio(login.Email);

            return dominio switch
            {
                "@est.biozin.edu.cr" => LoginEstudiante(login),
                "@admin.biozin.edu.cr" => LoginAdministrador(login),
                _ => Ok(new Respuesta<TAuthRespuesta> { blnError = true, strTituloRespuesta = "Dominio no reconocido", strMensajeRespuesta = "El correo ingresado no corresponde a ningún perfil del sistema." })
            };
        }

        [HttpPost("CambiarContrasenaTemporaria")]
        public IActionResult CambiarContrasenaTemporaria([FromBody] TCambioContrasena obj)
            => Ok(_portalEstudianteLN.CambiarContrasenaTemporaria(obj));

        private IActionResult LoginEstudiante(TLoginRequest login)
        {
            var credenciales = new TLoginEstudiante
            {
                EmailInstitucional = login.Email,
                Contrasena = login.Contrasena
            };

            var resultado = _portalEstudianteLN.Login(credenciales);

            if (resultado.blnError || resultado.ValorRetorno == null)
            {
                return Ok(new Respuesta<TAuthRespuesta>
                {
                    blnError = resultado.blnError,
                    strTituloRespuesta = resultado.strTituloRespuesta,
                    strMensajeRespuesta = resultado.strMensajeRespuesta
                });
            }

            var perfil = resultado.ValorRetorno;
            var token = GenerarToken(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, perfil.IdEstudiante.ToString()),
                new Claim(ClaimTypes.Name, $"{perfil.Nombre} {perfil.ApellidoPaterno}"),
                new Claim(ClaimTypes.Email, perfil.EmailInstitucional ?? ""),
                new Claim("carnet", perfil.Carnet.ToString()),
                new Claim(ClaimTypes.Role, "Estudiante")
            });

            return Ok(new Respuesta<TAuthRespuesta>
            {
                ValorRetorno = new TAuthRespuesta
                {
                    Token = token,
                    Role = "Estudiante",
                    Id = perfil.IdEstudiante,
                    Nombre = $"{perfil.Nombre} {perfil.ApellidoPaterno}",
                    Email = perfil.EmailInstitucional ?? "",
                    Carnet = perfil.Carnet,
                    IdCarrera = perfil.IdCarrera,
                    NombreCarrera = perfil.NombreCarrera,
                    SemestreActual = perfil.SemestreActual,
                    RequiereCambioContrasena = perfil.RequiereCambioContrasena
                }
            });
        }

        private IActionResult LoginAdministrador(TLoginRequest login)
        {
            var resultado = _administradorLN.Login(login.Email, login.Contrasena);

            if (resultado.blnError || resultado.ValorRetorno == null)
            {
                return Ok(new Respuesta<TAuthRespuesta>
                {
                    blnError = resultado.blnError,
                    strTituloRespuesta = resultado.strTituloRespuesta,
                    strMensajeRespuesta = resultado.strMensajeRespuesta
                });
            }

            var admin = resultado.ValorRetorno;
            var token = GenerarToken(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.IdAdministrador.ToString()),
                new Claim(ClaimTypes.Name, admin.NombreCompleto),
                new Claim(ClaimTypes.Email, admin.EmailInstitucional ?? ""),
                new Claim(ClaimTypes.Role, "Administrador")
            });

            return Ok(new Respuesta<TAuthRespuesta>
            {
                ValorRetorno = new TAuthRespuesta
                {
                    Token = token,
                    Role = "Administrador",
                    Id = admin.IdAdministrador,
                    Nombre = admin.NombreCompleto,
                    Email = admin.EmailInstitucional ?? ""
                }
            });
        }

        private static string ObtenerDominio(string email)
        {
            var at = email.IndexOf('@');
            return at >= 0 ? email[at..] : string.Empty;
        }

        private string GenerarToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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
