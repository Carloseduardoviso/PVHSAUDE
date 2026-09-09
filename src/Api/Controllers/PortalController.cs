using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/portal"), AllowAnonymous]
public class PortalController(Context context) : ControllerBase
{
    [HttpGet("planos-empresas")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> PlanosPorEmpresa(CancellationToken ct)
    {
        var resultados = await (
            from plano in context.Planos.AsNoTracking()
            join empresa in context.Credenciados.AsNoTracking() on plano.Id equals empresa.PlanoId
            where empresa.StatusCredenciamento == PVHSAUDE.Domain.Enuns.StatusCredenciamento.Ativo
            select new
            {
                CredenciadoId = empresa.Id,
                PlanoId = plano.Id,
                Plano = plano.Nome,
                plano.Valor,
                plano.Periodicidade
            }).Distinct().OrderBy(x => x.Plano).ThenBy(x => x.PlanoId).ToListAsync(ct);
        return Ok(resultados);
    }
}
