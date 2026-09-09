using Infra.Data.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Contracts;
using PVHSAUDE.Domain.Entities;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/especialidades")]
public class EspecialidadesController(Context db) : ControllerBase
{
 [HttpGet] public async Task<IActionResult> Get(CancellationToken ct)=>Ok(await db.Especialidades.AsNoTracking().Where(x=>x.Ativo).OrderBy(x=>x.Nome).Select(x=>new EspecialidadeResponse(x.Id,x.Nome,x.Ativo)).ToListAsync(ct));
 [Microsoft.AspNetCore.Authorization.Authorize, HttpPost] public async Task<IActionResult> Post(CatalogoRequest r,CancellationToken ct){var x=new Especialidade(r.Nome);db.Especialidades.Add(x);await db.SaveChangesAsync(ct);return Ok(new EspecialidadeResponse(x.Id,x.Nome,x.Ativo));}
}
