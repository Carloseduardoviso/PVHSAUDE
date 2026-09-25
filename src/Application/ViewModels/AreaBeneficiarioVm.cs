using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Application.ViewModels;

public class CadastroAcessoBeneficiarioVm
{
    [Required, RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})", ErrorMessage = "Informe um CPF completo.")]
    public string Cpf { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 8), DataType(DataType.Password)]
    public string Senha { get; set; } = "";
}

public record AcessoBeneficiarioPendenteVm(Guid UsuarioId, string Nome, string Email, string CpfSolicitado);
public record AcessoBeneficiarioEmitidoVm(string Nome, string Cpf, string SenhaTemporaria);

public class LoginCpfBeneficiarioVm
{
    [Required, RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})")]
    public string Cpf { get; set; } = "";
    [Required]
    public string Senha { get; set; } = "";
}

public record AreaDependenteVm(string Codigo, string Nome, string Cpf, DateTime DataNascimento, GrauParentesco GrauParentesco, DateTime DataValidade);

public record AreaBeneficiarioVm(
    Guid Id, string Codigo, string Nome, string Cpf, DateTime DataNascimento, string? Telefone,
    string? Email, string? Endereco, string PlanoNome, decimal? PlanoValor, Periodicidade PlanoPeriodicidade,
    DateTime DataInicio, DateTime DataValidade, StatusBeneficiario Status,
    IReadOnlyList<AreaDependenteVm> Dependentes, bool EhDependente, GrauParentesco? GrauParentesco, decimal? ValorDependente);
