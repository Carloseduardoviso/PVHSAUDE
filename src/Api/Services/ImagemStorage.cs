using PVHSAUDE.Application.Interface;
namespace PVHSAUDE.Api.Services;

public class ImagemStorage(IWebHostEnvironment environment) : IImagemStorage
{
    public async Task<string> SalvarCredenciadoAsync(Guid id, string extensao, Stream conteudo, CancellationToken ct)
    {
        var pasta = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "credenciados");
        Directory.CreateDirectory(pasta);
        var nome = $"{id:N}-{Guid.NewGuid():N}{extensao}";
        await using var destino = File.Create(Path.Combine(pasta, nome));
        await conteudo.CopyToAsync(destino, ct);
        return $"/uploads/credenciados/{nome}";
    }
}
