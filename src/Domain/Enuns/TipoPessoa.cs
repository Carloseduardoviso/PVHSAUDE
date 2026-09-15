using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Domain.Enuns;

public enum TipoPessoa
{
    [Display(Name = "Pessoa Física")]
    Fisica = 1,
    [Display(Name = "Empresarial")]
    Juridica = 2
}
