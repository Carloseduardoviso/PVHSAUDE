using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace PVHSAUDE.Api.Services;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string assunto, string mensagem, CancellationToken cancellationToken);
}

public sealed class EmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string assunto, string mensagem, CancellationToken cancellationToken)
    {
        var config = options.Value;
        if (!config.Enabled || string.IsNullOrWhiteSpace(config.EmailSenha))
            throw new InvalidOperationException("O envio de e-mail não está habilitado ou não possui credencial configurada.");

        using var email = new MailMessage(config.EmailRemetente, destinatario, assunto, mensagem)
        {
            IsBodyHtml = false,
            BodyEncoding = System.Text.Encoding.UTF8,
            SubjectEncoding = System.Text.Encoding.UTF8
        };
        using var smtp = new SmtpClient(config.EmailHost, config.EmailPorta)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(config.EmailRemetente, config.EmailSenha),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        await smtp.SendMailAsync(email, cancellationToken);
    }
}
