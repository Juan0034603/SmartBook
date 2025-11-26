using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartBook.Application.Interface;
using SmartBook.Application.Options;
using SmartBook.Domain.Dtos.Requests;
using SmartBook.Domain.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartBook.Application.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;


    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }


    

    public async Task<bool> EnviarCorreo(string destinatario, string asunto, string cuerpo)
    {
        try
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Puerto)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _smtpSettings.Usuario,
                    _smtpSettings.Contrasena
                )
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    _smtpSettings.CorreoRemitente,
                    _smtpSettings.NombreRemitente
                ),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            // ⬇️ Usar SendMailAsync en vez de SendAsync
            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation($"✅ Correo enviado a: {destinatario}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al enviar correo a: {destinatario}");
            return false;
        }
    }

}
