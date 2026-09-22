using PVHSAUDE.Application.Interface;
namespace PVHSAUDE.Api.Services;

public class ImagemStorage(IWebHostEnvironment environment) : IImagemStorage
{
    private string Pasta => Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "credenciados");

    public async Task<string> SalvarCredenciadoAsync(Guid id, string extensao, Stream conteudo, CancellationToken ct)
    {
        Directory.CreateDirectory(Pasta);
        var nome = $"{id:N}-{Guid.NewGuid():N}{extensao}";
        await using var destino = File.Create(Path.Combine(Pasta, nome));
        await conteudo.CopyToAsync(destino, ct);
        return $"/uploads/credenciados/{nome}";
    }

    public Task ExcluirCredenciadoAsync(string url, CancellationToken ct)
    {
        const string prefixo = "/uploads/credenciados/";
        if (url.StartsWith(prefixo, StringComparison.Ordinal))
        {
            var nome = url[prefixo.Length..];
            if (nome.Length > 0 && nome is not ("." or "..") &&
                nome.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'))
            {
                var pasta = Path.GetFullPath(Pasta);
                var arquivo = Path.GetFullPath(Path.Combine(pasta, nome));
                if (arquivo.StartsWith(pasta + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    File.Delete(arquivo);
            }
        }
        return Task.CompletedTask;
    }
}
