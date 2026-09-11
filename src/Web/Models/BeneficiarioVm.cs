using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Validation;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class BeneficiarioVm
{
    public Guid BeneficiarioId { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(18), RegularExpression(@"(?:\d{11}|\d{14}|\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})", ErrorMessage = "Informe um CPF ou CNPJ completo.")]
    [Display(Name = "CPF/CNPJ")]
    public string Cpf { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime DataNascimento { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [EmailAddress, Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [GuidNaoVazio, Display(Name = "Plano")]
    public Guid PlanoId { get; set; }

    [GuidNaoVazio(ErrorMessage = "Selecione a empresa credenciada.")]
    [Display(Name = "Empresa credenciada")]
    public Guid? CredenciadoId { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Início do benefício")]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Validade do benefício")]
    public DateTime DataValidade { get; set; } = DateTime.Today.AddYears(1);

    [Display(Name = "Situação")]
    public int Status { get; set; } = 2;

    [MaxLength(5, ErrorMessage = "É permitido cadastrar no máximo 5 dependentes.")]
    public List<DependenteViewModel> Dependentes { get; set; } = [];
}

public class DependenteViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})", ErrorMessage = "Informe um CPF completo.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataNascimento, DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [Required(ErrorMessage = "Selecione o parentesco."), EnumDataType(typeof(GrauParentesco), ErrorMessage = "Selecione um parentesco válido.")]
    public GrauParentesco? GrauParentesco { get; set; }
}
