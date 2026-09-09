using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/usuarios"), Authorize(Roles = "Administrador")]
public class UsuariosController(Context db, IPasswordHasher<Usuario> hasher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok(await db.Set<Usuario>().AsNoTracking().OrderBy(x => x.NomeCompleto)
            .Select(x => new UsuarioVm { UsuarioId = x.Id, NomeCompleto = x.NomeCompleto, Email = x.Email, Role = x.Role, Ativo = x.Ativo, Menus = PVHSAUDE.Domain.Enuns.MenusAdministrativos.Ler(x.MenusPermitidos) }).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var u = await db.Set<Usuario>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return u is null ? NotFound() : Ok(new UsuarioVm { UsuarioId = u.Id, NomeCompleto = u.NomeCompleto, Email = u.Email, Role = u.Role, Ativo = u.Ativo, Menus = PVHSAUDE.Domain.Enuns.MenusAdministrativos.Ler(u.MenusPermitidos) });
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> Editar(Guid id, UsuarioEdicaoVm model, CancellationToken ct) =>
        Alterar(id, model, null, false, ct);

    [HttpPost("{id:guid}/inativar")]
    public Task<IActionResult> Inativar(Guid id, CancellationToken ct) => Alterar(id, null, false, false, ct);

    [HttpPost("{id:guid}/ativar")]
    public Task<IActionResult> Ativar(Guid id, CancellationToken ct) => Alterar(id, null, true, false, ct);

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Excluir(Guid id, CancellationToken ct) => Alterar(id, null, null, true, ct);

    private async Task<IActionResult> Alterar(Guid id, UsuarioEdicaoVm? model, bool? ativo, bool excluir, CancellationToken ct)
    {
        // Serialize administrator changes, including concurrent requests.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync(
            "DECLARE @r int; EXEC @r = sp_getapplock @Resource = 'UsuariosAdministracao', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; IF @r < 0 THROW 50001, 'Não foi possível bloquear o cadastro de usuários.', 1;", ct);
        var usuario = await db.Set<Usuario>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (usuario is null) return NotFound();
        var administrador = PVHSAUDE.Domain.Enuns.Role.Administrador;
        if (usuario.Ativo && usuario.Role == administrador &&
            (excluir || ativo == false || (model is not null && model.Role != administrador)) &&
            !await db.Set<Usuario>().AnyAsync(x => x.Id != id && x.Ativo && x.Role == administrador, ct))
            return Conflict(new ProblemDetails { Detail = "Não é possível remover o último administrador ativo. Cadastre outro administrador antes." });
        if (excluir) db.Remove(usuario);
        else
        {
            if (ativo.HasValue) usuario.Ativo = ativo.Value;
            if (model is not null)
            {
                var normalizado = model.Email.Trim().ToUpperInvariant();
                if (await db.Set<Usuario>().AnyAsync(x => x.Id != id && x.EmailNormalizado == normalizado, ct))
                    return Conflict(new ProblemDetails { Detail = "Já existe um usuário com este e-mail." });
                usuario.NomeCompleto = model.NomeCompleto.Trim();
                usuario.Email = model.Email.Trim();
                usuario.EmailNormalizado = normalizado;
                usuario.Role = model.Role;
                usuario.MenusPermitidos = PVHSAUDE.Domain.Enuns.MenusAdministrativos.Gravar(model.Menus);
                if (!string.IsNullOrEmpty(model.Senha)) usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);
            }
        }
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { return Conflict(new ProblemDetails { Detail = "Já existe um usuário com este e-mail." }); }
        await transaction.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Criar(UsuarioCadastroVm model, CancellationToken ct)
    {
        var email = model.Email.Trim();
        var normalizado = email.ToUpperInvariant();
        if (await db.Set<Usuario>().AnyAsync(x => x.EmailNormalizado == normalizado, ct))
            return Conflict(new ProblemDetails { Detail = "Já existe um usuário com este e-mail." });
        var usuario = new Usuario { NomeCompleto = model.NomeCompleto.Trim(), Email = email, EmailNormalizado = normalizado, Role = model.Role };
        usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);
        usuario.MenusPermitidos = PVHSAUDE.Domain.Enuns.MenusAdministrativos.Gravar(model.Menus);
        db.Add(usuario);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { return Conflict(new ProblemDetails { Detail = "Já existe um usuário com este e-mail." }); }
        return StatusCode(201, new UsuarioVm { UsuarioId = usuario.Id, NomeCompleto = usuario.NomeCompleto, Email = usuario.Email, Role = usuario.Role, Menus = model.Menus });
    }
}
