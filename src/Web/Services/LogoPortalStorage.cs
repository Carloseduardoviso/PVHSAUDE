using PVHSAUDE.Application.AppService;

namespace Web.Services;

public sealed class LogoPortalStorage(IWebHostEnvironment environment)
{
    private static readonly Dictionary<string, string> Tipos = new(StringComparer.OrdinalIgnoreCase) { [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".webp"] = "image/webp" };
    private string Pasta => Path.Combine(environment.WebRootPath, "uploads", "logo-portal");
    public (string Path, string Tipo)? Obter()
    {
        if (!Directory.Exists(Pasta)) return null;
        var arquivo = Directory.EnumerateFiles(Pasta, "logo-portal.*").FirstOrDefault();
        return arquivo is not null && Tipos.TryGetValue(Path.GetExtension(arquivo), out var tipo) ? (arquivo, tipo) : null;
    }
    public async Task SalvarAsync(IFormFile logo, CancellationToken ct)
    {
        if (logo.Length is <= 0 or > 5_242_880) throw new InvalidOperationException("A logo deve ter até 5 MB.");
        var extensao = Path.GetExtension(logo.FileName);
        if (!Tipos.ContainsKey(extensao)) throw new InvalidOperationException("Envie uma logo JPG, PNG ou WEBP válida.");

        using var ms = new MemoryStream();
        await logo.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var (w, h) = BannerFormato.Dimensoes(bytes);
        if (w <= 0 || h <= 0)
            throw new InvalidOperationException("Não foi possível ler a imagem. Envie um JPG, PNG ou WEBP válido.");

        if (!LogoPortalFormato.Valido(bytes))
            throw new InvalidOperationException(LogoPortalFormato.Mensagem);

        Directory.CreateDirectory(Pasta);
        foreach (var anterior in Directory.EnumerateFiles(Pasta, "logo-portal.*")) File.Delete(anterior);
        await using var destino = File.Create(Path.Combine(Pasta, "logo-portal" + extensao.ToLowerInvariant()));
        await destino.WriteAsync(bytes, ct);
    }
}
