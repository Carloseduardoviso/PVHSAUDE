using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Web.Areas.Beneficiario.Models;

public class CadastroBeneficiarioContaVm
{
    [Required(ErrorMessage = "Informe seu CPF."), RegularExpression(@"(?:\d{11}|\d{3}\.\d{3}\.\d{3}-\d{2})", ErrorMessage = "Informe um CPF completo."), Display(Name = "CPF")]
    public string Cpf { get; set; } = "";
    [Required(ErrorMessage = "Crie uma senha."), StringLength(128, MinimumLength = 8), DataType(DataType.Password), Display(Name = "Senha")]
    public string Senha { get; set; } = "";
    [Required, Compare(nameof(Senha), ErrorMessage = "As senhas não conferem."), DataType(DataType.Password), Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = "";
}
