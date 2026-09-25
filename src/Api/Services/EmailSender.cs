using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PVHSAUDE.Api.Services;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string assunto, string mensagem, string mensagemHtml, CancellationToken cancellationToken);
}

public sealed class EmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, string assunto, string mensagem, string mensagemHtml, CancellationToken cancellationToken)
    {
        var config = options.Value;
        if (!config.Enabled || string.IsNullOrWhiteSpace(config.EmailSenha))
            throw new InvalidOperationException("O envio de e-mail não está habilitado ou não possui credencial configurada.");

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(config.EmailRemetente));
        email.To.Add(MailboxAddress.Parse(destinatario));
        email.Subject = assunto;
        email.Body = new BodyBuilder
        {
            TextBody = mensagem,
            HtmlBody = mensagemHtml
        }.ToMessageBody();

        var ssl = config.EmailSeguro
            ? config.EmailPorta == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(config.EmailHost, config.EmailPorta, ssl, cancellationToken);
        await smtp.AuthenticateAsync(config.EmailRemetente, config.EmailSenha, cancellationToken);
        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}
