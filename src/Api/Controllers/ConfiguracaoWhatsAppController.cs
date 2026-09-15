using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/configuracao-whatsapp")]
public class ConfiguracaoWhatsAppController(Context db) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var configuracao = await db.Set<ConfiguracaoWhatsApp>().AsNoTracking().SingleOrDefaultAsync(ct);
        return configuracao is null ? NotFound() : Ok(new ConfiguracaoWhatsAppVm
        { Nome = configuracao.Nome, Mensagem = configuracao.Mensagem, Telefone = configuracao.Telefone });
    }

    [HttpPut, Authorize]
    public async Task<IActionResult> Salvar(ConfiguracaoWhatsAppVm model, CancellationToken ct)
    {
        var configuracao = await db.Set<ConfiguracaoWhatsApp>().SingleOrDefaultAsync(ct);
        var novo = configuracao is null;
        configuracao ??= new ConfiguracaoWhatsApp();
        configuracao.Nome = model.Nome.Trim();
        configuracao.Mensagem = model.Mensagem.Trim();
        configuracao.Telefone = model.Telefone;
        if (novo) db.Add(configuracao);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (novo && ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        { return Conflict("A configuração já foi cadastrada. Recarregue a página para editar."); }
        return NoContent();
    }
}
