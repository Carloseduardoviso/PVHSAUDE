using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Domain.Validation;

public sealed class GuidNaoVazioAttribute : ValidationAttribute
{
    public GuidNaoVazioAttribute() : base("Informe um identificador válido para {0}.") { }
    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
}
