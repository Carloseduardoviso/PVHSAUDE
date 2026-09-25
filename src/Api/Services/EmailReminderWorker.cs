using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Api.Services;

public sealed class EmailReminderWorker(
    IServiceScopeFactory scopeFactory,
    IEmailSender emailSender,
    IOptions<EmailOptions> options,
    ILogger<EmailReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled || string.IsNullOrWhiteSpace(options.Value.EmailSenha))
        {
            logger.LogInformation("Lembretes por e-mail desabilitados: configure Email:Enabled e Email:EmailSenha.");
            return;
        }

        logger.LogInformation("Lembretes por e-mail ativos. Verificação inicial e recorrente a cada minuto.");
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try { await EnviarLembretesAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Falha ao processar lembretes de validade por e-mail."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task EnviarLembretesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();
        var hoje = DateTime.Today;
        var dias = options.Value.DiasAntesValidade.Distinct().Where(x => x >= 0 && x <= 365).ToArray();
        if (dias.Length == 0) return;

        var beneficiariosNaJanela = await db.Beneficiarios
            .AsNoTracking()
            .Include(x => x.Dependentes)
            .Where(x => x.DataValidade >= hoje && x.DataValidade < hoje.AddDays(dias.Max() + 1))
            .ToListAsync(ct);

        var beneficiariosElegiveis = beneficiariosNaJanela
            .Where(x => x.Status == StatusBeneficiario.Ativo || x.Status == StatusBeneficiario.EmRenovacao)
            .ToArray();
        logger.LogInformation(
            "Verificação de lembretes: hoje={Hoje}, naJanela={NaJanela}, elegiveis={Elegiveis}, offsets={Offsets}.",
            hoje, beneficiariosNaJanela.Count, beneficiariosElegiveis.Length, string.Join(",", dias));

        foreach (var titular in beneficiariosElegiveis)
        {
            var faltam = (titular.DataValidade.Date - hoje).Days;
            if (!dias.Contains(faltam)) continue;
            if (!string.IsNullOrWhiteSpace(titular.Email))
                await EnviarUmaVezAsync(db, titular.Id, titular.DataValidade, faltam, titular.Email,
                    titular.Nome, ct);
            else
                logger.LogWarning("Beneficiario {BeneficiarioId} elegivel para lembrete, mas sem e-mail cadastrado.", titular.Id);

            foreach (var dependente in titular.Dependentes.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
                await EnviarUmaVezAsync(db, dependente.Id, titular.DataValidade, faltam, dependente.Email!,
                    dependente.Nome, ct);
        }
    }

    private async Task EnviarUmaVezAsync(Context db, Guid pessoaId, DateTime validade, int diasAntes,
        string destinatario, string nome, CancellationToken ct)
    {
        var registro = await db.Set<EmailLembrete>().SingleOrDefaultAsync(x =>
            x.PessoaId == pessoaId && x.DataValidade == validade.Date && x.DiasAntes == diasAntes, ct);
        if (registro?.Enviado == true)
        {
            logger.LogInformation("Lembrete ignorado: pessoa {PessoaId}, validade {Validade}, marco {DiasAntes} dia(s) já enviado anteriormente.",
                pessoaId, validade.Date, diasAntes);
            return;
        }
        if (registro is null)
        {
            registro = new EmailLembrete(pessoaId, validade, diasAntes, destinatario);
            db.Add(registro);
            await db.SaveChangesAsync(ct);
        }

        var prazo = diasAntes == 0 ? "vence hoje" : $"vence em {diasAntes} dia(s)";
        var assunto = diasAntes == 0 ? "Sua carteirinha vence hoje" : $"Lembrete: sua carteirinha vence em {diasAntes} dia(s)";
        var corpo = $"Olá, {nome}.\n\nSua carteirinha PVH Saúde {prazo}, em {validade:dd/MM/yyyy}. Entre em contato com a PVH Saúde para verificar a renovação.\n\nEsta é uma mensagem automática.";
        var nomeHtml = System.Net.WebUtility.HtmlEncode(nome);
        var dataHtml = System.Net.WebUtility.HtmlEncode(validade.ToString("dd/MM/yyyy"));
        var prazoHtml = System.Net.WebUtility.HtmlEncode(prazo);
        var tituloHtml = diasAntes == 0 ? "Sua carteirinha vence hoje" : $"Sua carteirinha {prazoHtml}";
        var corpoHtml = $"""
            <!doctype html>
            <html lang="pt-BR">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#f2f5fa;font-family:Arial,Helvetica,sans-serif;color:#24324a;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f2f5fa;padding:32px 12px;">
                <tr><td align="center">
                  <table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:600px;background:#fff;border:1px solid #e1e8f2;border-radius:16px;overflow:hidden;">
                    <tr><td style="background:#15357a;padding:24px 32px;border-bottom:5px solid #72bf44;">
                      <div style="font-size:23px;line-height:1.2;font-weight:700;color:#fff;">PVH <span style="color:#a8e06b;">Sa&#250;de+</span></div>
                      <div style="margin-top:7px;font-size:12px;letter-spacing:1.2px;color:#dbe7ff;">LEMBRETE DA SUA CARTEIRINHA</div>
                    </td></tr>
                    <tr><td style="padding:32px;">
                      <p style="margin:0 0 8px;font-size:16px;color:#53627a;">Ol&#225;, {nomeHtml}!</p>
                      <h1 style="margin:0 0 14px;font-size:26px;line-height:1.25;color:#15357a;">{tituloHtml}</h1>
                      <p style="margin:0 0 24px;font-size:15px;line-height:1.6;color:#53627a;">Este &#233; um lembrete para voc&#234; se programar e manter seus benef&#237;cios em dia.</p>
                      <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f5f8fd;border:1px solid #dce6f4;border-radius:12px;">
                        <tr><td style="padding:18px 20px;">
                          <div style="font-size:12px;font-weight:700;letter-spacing:.7px;color:#65748b;">VALIDADE DA CARTEIRINHA</div>
                          <div style="margin-top:6px;font-size:22px;font-weight:700;color:#15357a;">{dataHtml}</div>
                          <div style="margin-top:5px;font-size:14px;color:#53627a;">{prazoHtml}</div>
                        </td></tr>
                      </table>
                      <p style="margin:24px 0 18px;font-size:15px;line-height:1.6;color:#53627a;">Para verificar a renova&#231;&#227;o, fale com a equipe da PVH Sa&#250;de.</p>
                      <table role="presentation" cellspacing="0" cellpadding="0" border="0"><tr><td style="background:#15357a;border-radius:8px;">
                        <a href="mailto:pvhsaude@pvhsaude.com.br" style="display:inline-block;padding:13px 20px;color:#fff;text-decoration:none;font-size:14px;font-weight:700;">Falar com a PVH Sa&#250;de</a>
                      </td></tr></table>
                    </td></tr>
                    <tr><td style="padding:18px 32px;background:#f8fafd;border-top:1px solid #e7edf5;font-size:12px;line-height:1.6;color:#748198;">
                      Esta &#233; uma mensagem autom&#225;tica da PVH Sa&#250;de. Em caso de d&#250;vida, responda a este e-mail ou entre em contato com nossa equipe.
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
        await emailSender.EnviarAsync(destinatario, assunto, corpo, corpoHtml, ct);
        registro.MarcarEnviado(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Lembrete de validade enviado para pessoa {PessoaId}, validade {Validade}, marco {DiasAntes} dia(s).",
            pessoaId, validade.Date, diasAntes);
    }
}
