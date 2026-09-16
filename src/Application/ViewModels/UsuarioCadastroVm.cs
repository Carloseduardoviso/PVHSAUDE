using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;

public class UsuarioCadastroVm
{
    [MenusValidos]
    public string[] Menus { get; set; } = [];
    [Required(ErrorMessage = "Informe o nome completo."), StringLength(200), Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = "";
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "Informe um e-mail válido."), StringLength(254), Display(Name = "E-mail")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Informe a senha."), StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 128 caracteres."), DataType(DataType.Password)]
    public string Senha { get; set; } = "";
    [EnumDataType(typeof(Role), ErrorMessage = "Selecione uma permissão válida."), Display(Name = "Permissão")]
    public Role Role { get; set; }
}
public class LoginVm
{
    [Required, EmailAddress, StringLength(254), Display(Name = "E-mail")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Informe sua senha."), StringLength(128), DataType(DataType.Password)]
    public string Senha { get; set; } = "";
}
public record LoginResponse(string Token, UsuarioVm Usuario);
