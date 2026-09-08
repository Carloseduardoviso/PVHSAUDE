using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class BeneficiarioViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Informe o nome."), Display(Name = "Nome completo")] public string Nome { get; set; } = string.Empty;
    [Required, StringLength(14), Display(Name = "CPF")] public string Cpf { get; set; } = string.Empty;
    [Required, DataType(DataType.Date), Display(Name = "Data de nascimento")] public DateTime DataNascimento { get; set; }
    [Display(Name = "Telefone")] public string? Telefone { get; set; }
    [EmailAddress, Display(Name = "E-mail")] public string? Email { get; set; }
    [Display(Name = "Endereço")] public string? Endereco { get; set; }
    [Range(1, int.MaxValue), Display(Name = "Plano")] public int PlanoId { get; set; } = 1;
    [Required, DataType(DataType.Date), Display(Name = "Início do benefício")] public DateTime DataInicio { get; set; } = DateTime.Today;
    [Required, DataType(DataType.Date), Display(Name = "Validade do benefício")] public DateTime DataValidade { get; set; } = DateTime.Today.AddYears(1);
    [Display(Name = "Situação")] public int Status { get; set; } = 2;
    public IReadOnlyCollection<DependenteViewModel> Dependentes { get; set; } = [];
}

public record DependenteViewModel(int Id, string Nome, string Cpf, DateTime DataNascimento, string GrauParentesco);
