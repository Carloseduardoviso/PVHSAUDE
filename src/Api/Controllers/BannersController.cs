using System.ComponentModel.DataAnnotations;
using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/banners")]
public class BannersController(Context db) : ControllerBase
{
    [HttpGet, Authorize]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(await Lista(false, ct));
    [HttpGet("ativos"), AllowAnonymous]
    public async Task<IActionResult> Ativos(CancellationToken ct) => Ok(await Lista(true, ct));
    private async Task<object> Lista(bool ativos, CancellationToken ct) =>
        await db.Set<Banner>().AsNoTracking().Where(x => !ativos || x.Ativo)
            .OrderByDescending(x => x.CriadoEm).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Titulo, x.Ativo }).ToListAsync(ct);

    [HttpGet("{id:guid}"), Authorize]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var b = await db.Set<Banner>().AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Id, x.Titulo, x.Ativo }).SingleOrDefaultAsync(ct);
        return b is null ? NotFound() : Ok(b);
    }
    [HttpGet("{id:guid}/imagem")]
    public async Task<IActionResult> Imagem(Guid id, CancellationToken ct)
    {
        var autenticado = User.Identity?.IsAuthenticated == true;
        var b = await db.Set<Banner>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && (x.Ativo || autenticado), ct);
        if (b is null) return NotFound();
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(b.Imagem, b.ContentType);
    }
    [HttpPost, Authorize, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Criar([FromForm] BannerRequest request, CancellationToken ct) => Salvar(null, request, ct);
    [HttpPut("{id:guid}"), Authorize, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Editar(Guid id, [FromForm] BannerRequest request, CancellationToken ct) => Salvar(id, request, ct);

    private async Task<IActionResult> Salvar(Guid? id, BannerRequest request, CancellationToken ct)
    {
        var b = id.HasValue ? await db.Set<Banner>().FindAsync(new object[] { id.Value }, ct) : new Banner();
        if (b is null) return NotFound();
        if (!id.HasValue && request.Imagem is null) return BadRequest("Selecione uma imagem.");
        if (request.Imagem is { } imagem)
        {
            if (imagem.Length == 0 || imagem.Length > 5_242_880) return BadRequest("A imagem deve ter até 5 MB.");
            await using var stream = new MemoryStream();
            await imagem.CopyToAsync(stream, ct);
            var bytes = stream.ToArray();
            var type = DetectarImagem(bytes);
            if (type is null) return BadRequest("Envie uma imagem JPG, PNG ou WEBP válida.");
            if (!PVHSAUDE.Api.Configs.BannerFormato.Valido(bytes)) return BadRequest(PVHSAUDE.Api.Configs.BannerFormato.Mensagem);
            b.Imagem = bytes;
            b.ContentType = type;
        }
        b.Titulo = request.Titulo.Trim();
        b.Ativo = request.Ativo;
        if (!id.HasValue) db.Add(b);
        await db.SaveChangesAsync(ct);
        return Ok(new { b.Id, b.Titulo, b.Ativo });
    }
    public static string? DetectarImagem(byte[] b)
    {
        if (b.Length >= 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) return "image/png";
        if (b.Length >= 3 && b[0] == 255 && b[1] == 216 && b[2] == 255) return "image/jpeg";
        if (b.Length >= 12 && System.Text.Encoding.ASCII.GetString(b,0,4) == "RIFF" && System.Text.Encoding.ASCII.GetString(b,8,4) == "WEBP") return "image/webp";
        return null;
    }
}
public class BannerRequest
{
    [Required, StringLength(200)] public string Titulo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public IFormFile? Imagem { get; set; }
}
