using Infra.Data.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Contracts;
using PVHSAUDE.Domain.Entities;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/procedimentos")]
public class ProcedimentosController(Context db) : ControllerBase
{
 [HttpGet] public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await db.Procedimentos.AsNoTracking().Where(x=>x.Ativo).OrderBy(x=>x.Nome).Select(x=>new ProcedimentoResponse(x.Id,x.Nome,x.Ativo)).ToListAsync(ct));
 [HttpPost] public async Task<IActionResult> Post(CatalogoRequest r,CancellationToken ct){var x=new Procedimento(r.Nome);db.Procedimentos.Add(x);await db.SaveChangesAsync(ct);return Ok(new ProcedimentoResponse(x.Id,x.Nome,x.Ativo));}
}
