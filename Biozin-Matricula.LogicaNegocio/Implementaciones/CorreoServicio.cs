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
            string correoRemitente,
            string urlCampus)
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
            html = html.Replace("{urlCampus}", urlCampus);
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587),
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
            string correoRemitente,
            string urlCampus)
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
            html = html.Replace("{urlCampus}", urlCampus);
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587),
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
            string correoRemitente,
            string urlCampus)
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
            html = html.Replace("{urlCampus}", urlCampus);
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;
            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587),
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
            string correoRemitente,
            string urlCampus)
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
            html = html.Replace("{urlCampus}", urlCampus);
            html = html.Replace("{anio}", DateTime.Now.Year.ToString());
            html = html.Replace("{nombreUniversidad}", nombreUniversidad);

            builder.HtmlBody = html;
            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }

        public async Task EnviarComprobanteMatriculaBulkAsync(
            string correoDestino,
            string nombre,
            long carnet,
            List<(string codigo, string nombreCurso, decimal monto)> cursos,
            string nombrePeriodo,
            DateTime fechaMatricula,
            decimal montoMatricula,
            decimal montoInfraestructura,
            decimal totalFinal,
            bool financiado,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(nombreUniversidad, correoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = $"Comprobante de Matrícula — {nombrePeriodo}";

            var filasCursos = string.Join("", cursos.Select(c => $@"
              <tr>
                <td style='padding:8px 12px;border-bottom:1px solid #e2e8f0;font-size:13px;color:#374151;'>{c.codigo}</td>
                <td style='padding:8px 12px;border-bottom:1px solid #e2e8f0;font-size:13px;color:#374151;'>{c.nombreCurso}</td>
                <td style='padding:8px 12px;border-bottom:1px solid #e2e8f0;font-size:13px;color:#374151;text-align:right;'>₡{c.monto:N2}</td>
              </tr>"));

            var estadoPago = financiado
                ? "<span style='background:#fef3c7;color:#92400e;padding:4px 10px;border-radius:20px;font-size:12px;font-weight:700;'>⏳ Pendiente de pago</span>"
                : "<span style='background:#d1fae5;color:#065f46;padding:4px 10px;border-radius:20px;font-size:12px;font-weight:700;'>✓ Pagado</span>";

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;background:#f8fafc;'>
  <div style='background:linear-gradient(135deg,#0f172a,#0891b2);padding:32px 40px;border-radius:12px 12px 0 0;'>
    <h1 style='color:white;margin:0;font-size:22px;letter-spacing:-0.5px;'>{nombreUniversidad}</h1>
    <p style='color:rgba(255,255,255,0.7);margin:6px 0 0;font-size:13px;'>Comprobante de Matrícula</p>
  </div>
  <div style='background:white;padding:32px 40px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.06);'>
    <p style='color:#374151;margin:0 0 24px;'>Hola <strong>{nombre}</strong>,</p>
    <p style='color:#374151;margin:0 0 20px;'>Tu matrícula para el período <strong>{nombrePeriodo}</strong> ha sido registrada exitosamente el {fechaMatricula.ToLocalTime():dd/MM/yyyy} a las {fechaMatricula.ToLocalTime():HH:mm}.</p>

    <div style='background:#f1f5f9;border-radius:8px;padding:4px 0;margin-bottom:20px;'>
      <p style='margin:12px 16px 6px;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;color:#64748b;'>Carnet</p>
      <p style='margin:0 16px 12px;font-size:18px;font-weight:800;color:#0f172a;'>{carnet}</p>
    </div>

    <table style='width:100%;border-collapse:collapse;margin-bottom:4px;'>
      <thead>
        <tr style='background:#f1f5f9;'>
          <th style='padding:8px 12px;text-align:left;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;color:#64748b;'>Código</th>
          <th style='padding:8px 12px;text-align:left;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;color:#64748b;'>Curso</th>
          <th style='padding:8px 12px;text-align:right;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;color:#64748b;'>Monto</th>
        </tr>
      </thead>
      <tbody>{filasCursos}</tbody>
    </table>

    <table style='width:100%;border-collapse:collapse;margin-bottom:24px;background:#f8fafc;border-radius:8px;'>
      <tr>
        <td style='padding:8px 12px;font-size:13px;color:#64748b;'>Derecho de matrícula</td>
        <td style='padding:8px 12px;font-size:13px;color:#64748b;text-align:right;'>₡{montoMatricula:N2}</td>
      </tr>
      <tr>
        <td style='padding:8px 12px;font-size:13px;color:#64748b;'>Infraestructura</td>
        <td style='padding:8px 12px;font-size:13px;color:#64748b;text-align:right;'>₡{montoInfraestructura:N2}</td>
      </tr>
      <tr style='border-top:2px solid #e2e8f0;'>
        <td style='padding:12px 12px 8px;font-size:15px;font-weight:800;color:#0f172a;'>Total</td>
        <td style='padding:12px 12px 8px;font-size:15px;font-weight:800;color:#0891b2;text-align:right;'>₡{totalFinal:N2}</td>
      </tr>
    </table>

    <div style='margin-bottom:24px;'>Estado del pago: {estadoPago}</div>

    <p style='color:#6b7280;font-size:12px;margin:0;border-top:1px solid #e2e8f0;padding-top:20px;'>
      © {DateTime.Now.Year} {nombreUniversidad} · Este es un correo automático, no responder.
    </p>
  </div>
</div>";

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Mail:Smtp"], (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }

        public async Task EnviarCodigoRecuperacionAsync(
            string correoDestino,
            string nombre,
            string codigo,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(nombreUniversidad, correoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = "Código de recuperación de contraseña";

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
<div style='font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:32px;background:#f9fafb;border-radius:12px;'>
  <h2 style='color:#1e3a5f;margin-bottom:8px;'>{nombreUniversidad}</h2>
  <p style='color:#374151;'>Hola <strong>{nombre}</strong>,</p>
  <p style='color:#374151;'>Recibimos una solicitud para restablecer tu contraseña. Usa el siguiente código:</p>
  <div style='text-align:center;margin:32px 0;'>
    <span style='font-size:40px;font-weight:bold;letter-spacing:10px;color:#06b6d4;background:#e0f7fa;padding:16px 32px;border-radius:8px;'>{codigo}</span>
  </div>
  <p style='color:#6b7280;font-size:13px;'>Este código expira en <strong>15 minutos</strong>. Si no solicitaste este cambio, ignora este correo.</p>
  <p style='color:#6b7280;font-size:12px;margin-top:24px;'>© {DateTime.Now.Year} {nombreUniversidad}</p>
</div>";

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Mail:Smtp"],
                (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587),
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }

        public async Task EnviarNotificacionPagoVencidoAsync(
            string correoDestino,
            string nombre,
            long carnet,
            string concepto,
            string nombrePeriodo,
            decimal monto,
            DateTime fechaVencimiento,
            List<string> nombresCursos,
            string nombreUniversidad,
            string correoRemitente)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(nombreUniversidad, correoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = $"Pago vencido — {nombrePeriodo}";

            var filasCursos = string.Join("", nombresCursos.Select(c => $@"
              <tr>
                <td style='padding:8px 12px;border-bottom:1px solid #fecaca;font-size:13px;color:#374151;'>📚 {c}</td>
              </tr>"));

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;background:#f8fafc;'>
  <div style='background:linear-gradient(135deg,#7f1d1d,#dc2626);padding:32px 40px;border-radius:12px 12px 0 0;'>
    <h1 style='color:white;margin:0;font-size:22px;letter-spacing:-0.5px;'>{nombreUniversidad}</h1>
    <p style='color:rgba(255,255,255,0.8);margin:6px 0 0;font-size:13px;'>Notificación de pago vencido</p>
  </div>
  <div style='background:white;padding:32px 40px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.06);'>
    <p style='color:#374151;margin:0 0 16px;'>Hola <strong>{nombre}</strong>,</p>
    <div style='background:#fef2f2;border-left:4px solid #dc2626;padding:16px 20px;border-radius:0 8px 8px 0;margin-bottom:24px;'>
      <p style='margin:0;color:#991b1b;font-weight:700;font-size:15px;'>⚠️ Tu pago ha vencido</p>
      <p style='margin:8px 0 0;color:#7f1d1d;font-size:13px;'>
        El plazo para realizar el pago de tu matrícula en el período <strong>{nombrePeriodo}</strong> expiró el {fechaVencimiento.ToLocalTime():dd/MM/yyyy}.
        Los cursos asociados han sido marcados como <strong>reprobados</strong>.
      </p>
    </div>

    <div style='background:#f1f5f9;border-radius:8px;padding:4px 0;margin-bottom:20px;'>
      <p style='margin:12px 16px 6px;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;color:#64748b;'>Carnet</p>
      <p style='margin:0 16px 12px;font-size:18px;font-weight:800;color:#0f172a;'>{carnet}</p>
    </div>

    <p style='font-size:13px;font-weight:700;color:#374151;margin:0 0 8px;text-transform:uppercase;letter-spacing:0.5px;'>Cursos afectados</p>
    <table style='width:100%;border-collapse:collapse;margin-bottom:20px;background:#fff5f5;border-radius:8px;overflow:hidden;'>
      <tbody>{filasCursos}</tbody>
    </table>

    <div style='background:#f8fafc;border-radius:8px;padding:16px;margin-bottom:24px;'>
      <div style='display:flex;justify-content:space-between;'>
        <span style='font-size:14px;color:#64748b;'>Concepto</span>
        <span style='font-size:14px;color:#374151;'>{concepto}</span>
      </div>
      <div style='display:flex;justify-content:space-between;margin-top:8px;border-top:1px solid #e2e8f0;padding-top:8px;'>
        <span style='font-size:15px;font-weight:800;color:#0f172a;'>Monto no pagado</span>
        <span style='font-size:15px;font-weight:800;color:#dc2626;'>₡{monto:N2}</span>
      </div>
    </div>

    <p style='color:#374151;font-size:13px;'>Si crees que esto es un error o deseas más información, comunícate con la oficina de registro de {nombreUniversidad}.</p>

    <p style='color:#6b7280;font-size:12px;margin:0;border-top:1px solid #e2e8f0;padding-top:20px;'>
      © {DateTime.Now.Year} {nombreUniversidad} · Este es un correo automático, no responder.
    </p>
  </div>
</div>";

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Mail:Smtp"], (int.TryParse(_config["Mail:Puerto"], out var smtpPuerto) ? smtpPuerto : 587), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Mail:Usuario"], _config["Mail:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }
    }
}