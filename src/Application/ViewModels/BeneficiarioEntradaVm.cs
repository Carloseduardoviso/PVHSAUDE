using PVHSAUDE.Domain.Validation;
using PVHSAUDE.Domain.Enuns;
using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Application.ViewModels;

public class BeneficiarioEntradaVm
{
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [Required, StringLength(18), RegularExpression(@"(?:\d{11}|\d{14}|\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})")] public string Cpf { get; set; } = string.Empty;
    [DataType(DataType.Date)] public DateTime DataNascimento { get; set; }
    [StringLength(20)] public string? Telefone { get; set; }
    [EmailAddress] public string? Email { get; set; }
    [StringLength(250)] public string? Endereco { get; set; }
    public Guid PlanoId { get; set; }
    public Guid? CredenciadoId { get; set; }
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Fisica;
    public Guid? EmpresaBeneficiadaId { get; set; }
    [DataType(DataType.Date)] public DateTime DataInicio { get; set; }
    [DataType(DataType.Date)] public DateTime DataValidade { get; set; }
    [MaxLength(5, ErrorMessage = "É permitido cadastrar no máximo 5 dependentes.")]
    public List<DependenteEntradaVm>? Dependentes { get; set; }
    public StatusBeneficiario Status { get; set; } = StatusBeneficiario.Pendente;
}

public record DependenteRespostaVm(Guid Id, string Codigo, string Nome, string Cpf, DateTime DataNascimento, GrauParentesco GrauParentesco);

public record BeneficiarioRespostaVm(Guid Id, string Codigo, string Nome, string Cpf, DateTime DataNascimento,
    string? Telefone, string? Email, string? Endereco, Guid PlanoId, DateTime DataInicio,
    DateTime DataValidade, StatusBeneficiario Status, IReadOnlyCollection<DependenteRespostaVm> Dependentes, Guid? CredenciadoId, TipoPessoa TipoPessoa, Guid? EmpresaBeneficiadaId);

public class DependenteEntradaVm
{
    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [Required, RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})")] public string Cpf { get; set; } = string.Empty;
    [Required(ErrorMessage = "Informe a data de nascimento."), DataNascimento] public DateTime? DataNascimento { get; set; }
    [Required(ErrorMessage = "Selecione o parentesco."), EnumDataType(typeof(GrauParentesco), ErrorMessage = "Selecione um parentesco válido.")] public GrauParentesco? GrauParentesco { get; set; }
}

