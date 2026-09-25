using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Web.Areas.Beneficiario.Models;

public class LoginCpfBeneficiarioContaVm
{
    [Required(ErrorMessage = "Informe seu CPF."), RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})", ErrorMessage = "Informe um CPF completo."), Display(Name = "CPF")]
    public string Cpf { get; set; } = "";

    [Required(ErrorMessage = "Informe sua senha."), DataType(DataType.Password), Display(Name = "Senha de acesso")]
    public string Senha { get; set; } = "";
}
