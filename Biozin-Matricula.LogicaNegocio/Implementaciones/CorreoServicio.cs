using Biozin_Matricula.Dominio.InterfacesLN;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;


namespace Biozin_Matricula.LogicaNegocio.Implementaciones
{
    public class CorreoServicio : ICorreoServicio
    {
        private readonly IConfiguration _config;

        public CorreoServicio(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCredencialesAsync(
            string correoDestino,
            string nombre,
            long carnet,
            string correoInstitucional,
            string password)
        {
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(
                _config["Mail:NombreRemitente"],
                _config["Mail:Remitente"]
            ));

            mensaje.To.Add(MailboxAddress.Parse(correoDestino));

            mensaje.Subject = "Credenciales de acceso al Campus Biozin";

            var builder = new BodyBuilder();

            string ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "PlantillasEmail",
                "credenciales.html"
            );

            string html = File.ReadAllText(ruta);

            html = html.Replace("{nombre}", nombre);
            html = html.Replace("{carnet}", carnet.ToString());
            html = html.Replace("{correoInstitucional}", correoInstitucional);
            html = html.Replace("{password}", password);
            html = html.Replace("{urlCampus}", "https://campus.biozin.edu");
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());

            builder.HtmlBody = html;

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                int.Parse(_config["Mail:Puerto"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _config["Mail:Usuario"],
                _config["Mail:Password"]
            );

            await smtp.SendAsync(mensaje);

            await smtp.DisconnectAsync(true);
        }
    }
}