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
        await emailSender.EnviarAsync(destinatario, assunto, corpo, ct);
        registro.MarcarEnviado(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Lembrete de validade enviado para pessoa {PessoaId}, validade {Validade}, marco {DiasAntes} dia(s).",
            pessoaId, validade.Date, diasAntes);
    }
}
