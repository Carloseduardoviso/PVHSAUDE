namespace PVHSAUDE.Domain.Entities;

public class ConfiguracaoWhatsApp
{
    public int Id { get; set; } = 1;
    public string Nome { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}
