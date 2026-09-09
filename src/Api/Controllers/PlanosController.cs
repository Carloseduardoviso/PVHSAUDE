using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Contracts;
using PVHSAUDE.Domain.Entities;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/planos")]
public class PlanosController(Context context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok((await context.Planos.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct)).Select(ParaResponse));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var plano = await context.Planos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return plano is null ? NotFound() : Ok(ParaResponse(plano));
    }

    [Microsoft.AspNetCore.Authorization.Authorize, HttpPost]
    public async Task<IActionResult> Criar(PlanoRequest request, CancellationToken ct)
    {
        var plano = new Plano(request.Nome, request.Descricao, request.Valor, request.Periodicidade, request.DataValidade);
        context.Planos.Add(plano);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = plano.Id }, ParaResponse(plano));
    }

    [Microsoft.AspNetCore.Authorization.Authorize, HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, PlanoRequest request, CancellationToken ct)
    {
        var plano = await context.Planos.FindAsync([id], ct);
        if (plano is null) return NotFound();
        plano.Atualizar(request.Nome, request.Descricao, request.Valor, request.Periodicidade, request.DataValidade);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static PlanoResponse ParaResponse(Plano p) =>
        new(p.Id, p.Nome, p.Descricao, p.Valor, p.Periodicidade, p.DataValidade);
}
