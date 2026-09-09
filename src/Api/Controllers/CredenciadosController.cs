using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Contracts;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/credenciados")]
public class CredenciadosController(Context context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok((await context.Credenciados.AsNoTracking().Include(x=>x.Especialidades).Include(x=>x.Procedimentos).OrderBy(x => x.NomeFantasia).ToListAsync(ct)).Select(ParaResponse));
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var empresa = await context.Credenciados.AsNoTracking().Include(x=>x.Especialidades).Include(x=>x.Procedimentos).FirstOrDefaultAsync(x => x.Id == id, ct);
        return empresa is null ? NotFound() : Ok(ParaResponse(empresa));
    }
    [Microsoft.AspNetCore.Authorization.Authorize, HttpPost]
    public async Task<IActionResult> Criar(CredenciadoRequest request, CancellationToken ct)
    {
        if (!await context.Planos.AnyAsync(x => x.Id == request.PlanoId, ct)) return BadRequest("Selecione um plano cadastrado.");
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        if (await context.Credenciados.AnyAsync(x => x.Cnpj == cnpj, ct))
            return Conflict("Já existe uma empresa credenciada com este CNPJ.");
        var empresa = new Credenciado(request.RazaoSocial, request.NomeFantasia, request.Cnpj, request.Telefone, request.WhatsApp, request.Email, request.Cep, request.Endereco, request.Cidade, request.Uf, request.Observacoes, request.Tipo!.Value, request.StatusCredenciamento!.Value);
        empresa.DefinirPlano(request.PlanoId!.Value);
        context.Credenciados.Add(empresa);
        empresa.Especialidades.ToList().ForEach(x => context.CredenciadoEspecialidades.Remove(x));
        foreach (var id in request.EspecialidadeIds.Distinct()) context.CredenciadoEspecialidades.Add(new CredenciadoEspecialidade(empresa.Id, id));
        foreach (var id in request.ProcedimentoIds.Distinct()) context.CredenciadoProcedimentos.Add(new CredenciadoProcedimento(empresa.Id, id));
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obter), new { id = empresa.Id }, ParaResponse(empresa));
    }
    [Microsoft.AspNetCore.Authorization.Authorize, HttpPost("{id:guid}/imagem")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadImagem(Guid id, IFormFile imagem, IWebHostEnvironment environment, CancellationToken ct)
    {
        var empresa = await context.Credenciados.Include(x=>x.Especialidades).Include(x=>x.Procedimentos).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if (empresa is null) return NotFound();
        var extensoes = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extensao = Path.GetExtension(imagem.FileName).ToLowerInvariant();
        if (imagem.Length == 0 || imagem.Length > 5_242_880 || !extensoes.Contains(extensao))
            return BadRequest("Envie uma imagem JPG, PNG ou WEBP de até 5 MB.");
        var pasta = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "credenciados");
        Directory.CreateDirectory(pasta);
        var nome = $"{id:N}-{Guid.NewGuid():N}{extensao}";
        await using var stream = System.IO.File.Create(Path.Combine(pasta, nome));
        await imagem.CopyToAsync(stream, ct);
        empresa.DefinirImagem($"/uploads/credenciados/{nome}");
        await context.SaveChangesAsync(ct);
        return Ok(new { empresa.ImagemUrl });
    }

    [Microsoft.AspNetCore.Authorization.Authorize, HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, CredenciadoRequest request, CancellationToken ct)
    {
        var empresa = await context.Credenciados.Include(x=>x.Especialidades).Include(x=>x.Procedimentos).FirstOrDefaultAsync(x=>x.Id==id,ct);
        if (empresa is null) return NotFound();
        if (!await context.Planos.AnyAsync(x => x.Id == request.PlanoId, ct)) return BadRequest("Selecione um plano cadastrado.");
        var cnpj = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        if (await context.Credenciados.AnyAsync(x => x.Id != id && x.Cnpj == cnpj, ct))
            return Conflict("Já existe uma empresa credenciada com este CNPJ.");
        empresa.Atualizar(request.RazaoSocial, request.NomeFantasia, request.Cnpj, request.Telefone, request.WhatsApp, request.Email, request.Cep, request.Endereco, request.Cidade, request.Uf, request.Observacoes, request.Tipo!.Value, request.StatusCredenciamento!.Value);
        empresa.DefinirPlano(request.PlanoId!.Value);
        context.CredenciadoEspecialidades.RemoveRange(empresa.Especialidades);
        context.CredenciadoProcedimentos.RemoveRange(empresa.Procedimentos);
        foreach (var especialidadeId in request.EspecialidadeIds.Distinct()) context.CredenciadoEspecialidades.Add(new CredenciadoEspecialidade(id, especialidadeId));
        foreach (var procedimentoId in request.ProcedimentoIds.Distinct()) context.CredenciadoProcedimentos.Add(new CredenciadoProcedimento(id, procedimentoId));
        await context.SaveChangesAsync(ct);
        return NoContent();
    }
    private CredenciadoResponse ParaResponse(Credenciado x) =>
        new(x.Id, x.RazaoSocial, x.NomeFantasia, x.Cnpj, x.Telefone, x.WhatsApp, x.Email, x.Cep, x.Endereco, x.Cidade, x.Uf, x.Observacoes, x.Tipo, x.StatusCredenciamento, x.ImagemUrl is null ? null : $"{Request.Scheme}://{Request.Host}{x.ImagemUrl}", x.Especialidades.Select(e=>e.EspecialidadeId).ToList(), x.Procedimentos.Select(e=>e.ProcedimentoId).ToList(), x.PlanoId);
}
