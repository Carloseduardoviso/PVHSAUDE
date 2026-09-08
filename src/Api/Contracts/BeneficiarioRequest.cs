using PVHSAUDE.Domain.Enuns;
using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Api.Contracts;

public class BeneficiarioRequest
{
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [Required, StringLength(14)] public string Cpf { get; set; } = string.Empty;
    [DataType(DataType.Date)] public DateTime DataNascimento { get; set; }
    [StringLength(20)] public string? Telefone { get; set; }
    [EmailAddress] public string? Email { get; set; }
    [StringLength(250)] public string? Endereco { get; set; }
    [Range(1, int.MaxValue)] public int PlanoId { get; set; }
    [DataType(DataType.Date)] public DateTime DataInicio { get; set; }
    [DataType(DataType.Date)] public DateTime DataValidade { get; set; }
    public StatusBeneficiario Status { get; set; } = StatusBeneficiario.Pendente;
}

public record DependenteResponse(int Id, string Nome, string Cpf, DateTime DataNascimento, string GrauParentesco);

public record BeneficiarioResponse(int Id, string Nome, string Cpf, DateTime DataNascimento,
    string? Telefone, string? Email, string? Endereco, int PlanoId, DateTime DataInicio,
    DateTime DataValidade, StatusBeneficiario Status, IReadOnlyCollection<DependenteResponse> Dependentes);
