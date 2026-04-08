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
            string password,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(
                nombreUniversidad,
                correoRemitente
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

            var rutaLogo = Path.Combine(Directory.GetCurrentDirectory(), "PlantillasEmail", "Biozin_logo.png");
            if (File.Exists(rutaLogo))
            {
                var imagen = builder.LinkedResources.Add(rutaLogo);
                imagen.ContentId = "logo-universidad";
                html = html.Replace("{logoUrl}", $"cid:{imagen.ContentId}");
            }
            else
            {
                html = html.Replace("{logoUrl}", string.Empty);
            }

            html = html.Replace("{nombre}", nombre);
            html = html.Replace("{carnet}", carnet.ToString());
            html = html.Replace("{correoInstitucional}", correoInstitucional);
            html = html.Replace("{password}", password);
            html = html.Replace("{urlCampus}", "http://localhost:4200/");
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

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

        public async Task EnviarCredencialesStaffAsync(
            string correoDestino,
            string nombre,
            string correoInstitucional,
            string password,
            string rol,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(
                nombreUniversidad,
                correoRemitente
            ));

            mensaje.To.Add(MailboxAddress.Parse(correoDestino));

            mensaje.Subject = "Credenciales de acceso al Campus Biozin";

            var builder = new BodyBuilder();

            string ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "PlantillasEmail",
                "credenciales-staff.html"
            );

            string html = File.ReadAllText(ruta);

            var rutaLogo = Path.Combine(Directory.GetCurrentDirectory(), "PlantillasEmail", "Biozin_logo.png");
            if (File.Exists(rutaLogo))
            {
                var imagen = builder.LinkedResources.Add(rutaLogo);
                imagen.ContentId = "logo-universidad";
                html = html.Replace("{logoUrl}", $"cid:{imagen.ContentId}");
            }
            else
            {
                html = html.Replace("{logoUrl}", string.Empty);
            }

            html = html.Replace("{nombre}", nombre);
            html = html.Replace("{correoInstitucional}", correoInstitucional);
            html = html.Replace("{password}", password);
            html = html.Replace("{rol}", rol);
            html = html.Replace("{urlCampus}", "http://localhost:4200/");
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

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

        public async Task EnviarComprobanteMatriculaAsync(
            string correoDestino,
            string nombre,
            long carnet,
            string codigoCurso,
            string nombreCurso,
            string nombrePeriodo,
            DateTime fechaMatricula,
            decimal monto,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(nombreUniversidad, correoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = "Comprobante de Matrícula - " + nombreCurso;

            var builder = new BodyBuilder();

            string ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "PlantillasEmail",
                "comprobante-matricula.html"
            );

            string html = File.ReadAllText(ruta);

            var rutaLogo = Path.Combine(Directory.GetCurrentDirectory(), "PlantillasEmail", "Biozin_logo.png");
            if (File.Exists(rutaLogo))
            {
                var imagen = builder.LinkedResources.Add(rutaLogo);
                imagen.ContentId = "logo-universidad";
                html = html.Replace("{logoUrl}", $"cid:{imagen.ContentId}");
            }
            else
            {
                html = html.Replace("{logoUrl}", string.Empty);
            }

            html = html.Replace("{nombre}", nombre);
            html = html.Replace("{carnet}", carnet.ToString());
            html = html.Replace("{codigoCurso}", codigoCurso);
            html = html.Replace("{nombreCurso}", nombreCurso);
            html = html.Replace("{nombrePeriodo}", nombrePeriodo);
            html = html.Replace("{fechaMatricula}", fechaMatricula.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
            html = html.Replace("{monto}", monto.ToString("N2"));
            html = html.Replace("{urlCampus}", "http://localhost:4200/");
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;
            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                int.Parse(_config["Mail:Puerto"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }

        public async Task EnviarComprobantePagoAsync(
            string correoDestino,
            string nombre,
            long carnet,
            int numeroPago,
            string concepto,
            string nombrePeriodo,
            decimal monto,
            DateTime fechaPago,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(nombreUniversidad, correoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = "Comprobante de Pago N.° " + numeroPago;

            var builder = new BodyBuilder();

            string ruta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "PlantillasEmail",
                "comprobante-pago.html"
            );

            string html = File.ReadAllText(ruta);

            var rutaLogo = Path.Combine(Directory.GetCurrentDirectory(), "PlantillasEmail", "Biozin_logo.png");
            if (File.Exists(rutaLogo))
            {
                var imagen = builder.LinkedResources.Add(rutaLogo);
                imagen.ContentId = "logo-universidad";
                html = html.Replace("{logoUrl}", $"cid:{imagen.ContentId}");
            }
            else
            {
                html = html.Replace("{logoUrl}", string.Empty);
            }

            html = html.Replace("{nombre}", nombre);
            html = html.Replace("{carnet}", carnet.ToString());
            html = html.Replace("{numeroPago}", numeroPago.ToString());
            html = html.Replace("{concepto}", concepto);
            html = html.Replace("{nombrePeriodo}", nombrePeriodo);
            html = html.Replace("{monto}", monto.ToString("N2"));
            html = html.Replace("{fechaPago}", fechaPago.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
            html = html.Replace("{urlCampus}", "http://localhost:4200/");
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;
            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                int.Parse(_config["Mail:Puerto"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }
    }
}