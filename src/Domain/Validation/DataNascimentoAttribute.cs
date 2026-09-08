using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Domain.Validation;

public sealed class DataNascimentoAttribute : ValidationAttribute
{
    public DataNascimentoAttribute() : base("Informe uma data de nascimento válida, entre 01/01/1900 e hoje.") { }

    public override bool IsValid(object? value) =>
        value is null || value is DateTime data && data >= new DateTime(1900, 1, 1) && data.Date <= DateTime.Today;
}
