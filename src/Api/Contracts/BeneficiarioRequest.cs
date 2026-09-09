using PVHSAUDE.Domain.Validation;
using PVHSAUDE.Domain.Enuns;
using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Api.Contracts;

public class BeneficiarioRequest
{
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [Required, StringLength(18), RegularExpression(@"(?:\d{11}|\d{14}|\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})")] public string Cpf { get; set; } = string.Empty;
    [DataType(DataType.Date)] public DateTime DataNascimento { get; set; }
    [StringLength(20)] public string? Telefone { get; set; }
    [EmailAddress] public string? Email { get; set; }
    [StringLength(250)] public string? Endereco { get; set; }
    public Guid PlanoId { get; set; }
    public Guid? CredenciadoId { get; set; }
    [DataType(DataType.Date)] public DateTime DataInicio { get; set; }
    [DataType(DataType.Date)] public DateTime DataValidade { get; set; }
    [MaxLength(5, ErrorMessage = "É permitido cadastrar no máximo 5 dependentes.")]
    public List<DependenteRequest>? Dependentes { get; set; }
    public StatusBeneficiario Status { get; set; } = StatusBeneficiario.Pendente;
}

public record DependenteResponse(Guid Id, string Nome, string Cpf, DateTime DataNascimento, GrauParentesco GrauParentesco);

public record BeneficiarioResponse(Guid Id, string Nome, string Cpf, DateTime DataNascimento,
    string? Telefone, string? Email, string? Endereco, Guid PlanoId, DateTime DataInicio,
    DateTime DataValidade, StatusBeneficiario Status, IReadOnlyCollection<DependenteResponse> Dependentes, Guid? CredenciadoId);

public class DependenteRequest
{
    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [Required, RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})")] public string Cpf { get; set; } = string.Empty;
    [Required(ErrorMessage = "Informe a data de nascimento."), DataNascimento] public DateTime? DataNascimento { get; set; }
    [Required(ErrorMessage = "Selecione o parentesco."), EnumDataType(typeof(GrauParentesco), ErrorMessage = "Selecione um parentesco válido.")] public GrauParentesco? GrauParentesco { get; set; }
}

