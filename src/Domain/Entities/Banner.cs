namespace PVHSAUDE.Domain.Entities;
public class Banner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titulo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public byte[] Imagem { get; set; } = [];
    public string ContentType { get; set; } = "";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
