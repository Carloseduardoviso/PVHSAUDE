using Infra.Data.Base; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Controllers;
[ApiController,Route("api/contatos")] public class ContatosController(Context db):ControllerBase{
[HttpGet,Authorize(Roles="Administrador")] public async Task<IActionResult> Listar(CancellationToken ct)=>Ok(await db.Set<Contato>().AsNoTracking().OrderByDescending(x=>x.EnviadoEm).ToListAsync(ct));
[HttpGet("{id:guid}"),Authorize(Roles="Administrador")] public async Task<IActionResult> Obter(Guid id,CancellationToken ct){var c=await db.Set<Contato>().AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);return c is null?NotFound():Ok(c);}
[HttpDelete("{id:guid}"),Authorize(Roles="Administrador")] public async Task<IActionResult> Excluir(Guid id,CancellationToken ct){var c=await db.Set<Contato>().FindAsync(new object[]{id},ct);if(c is null)return NotFound();db.Remove(c);await db.SaveChangesAsync(ct);return NoContent();}
[HttpPost,AllowAnonymous] public async Task<IActionResult> Criar(Contato contato,CancellationToken ct){contato.Id=Guid.NewGuid();contato.EnviadoEm=DateTime.UtcNow;db.Add(contato);await db.SaveChangesAsync(ct);return Ok();}
}
