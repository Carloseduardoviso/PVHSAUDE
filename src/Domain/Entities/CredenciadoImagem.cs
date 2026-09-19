namespace PVHSAUDE.Domain.Entities;

public class CredenciadoImagem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CredenciadoId { get; private set; }
    public Credenciado Credenciado { get; private set; } = null!;
    public string Url { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    private CredenciadoImagem() { }

    public CredenciadoImagem(Guid credenciadoId, string url, DateTime? criadoEm = null)
    {
        if (credenciadoId == Guid.Empty) throw new ArgumentException("Informe o credenciado da imagem.", nameof(credenciadoId));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Informe a URL da imagem.", nameof(url));

        CredenciadoId = credenciadoId;
        Url = url.Trim();
        CriadoEm = criadoEm?.ToUniversalTime() ?? DateTime.UtcNow;
    }
}
