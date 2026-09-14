using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Application.ViewModels;

public class ConfiguracaoWhatsAppVm
{
    [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a mensagem."), StringLength(2000)]
    public string Mensagem { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone."), StringLength(15)]
    [RegularExpression(@"[1-9][0-9]{9,14}", ErrorMessage = "Use somente números, com código do país e DDD. Exemplo: 5569992341486.")]
    public string Telefone { get; set; } = string.Empty;

    public string CriarLink() => $"https://wa.me/{Telefone}?text={Uri.EscapeDataString($"{Nome.Trim()}\n{Mensagem.Trim()}")}";
}
