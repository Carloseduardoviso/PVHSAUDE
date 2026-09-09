using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;

public class UsuarioEdicaoVm
{
    public Guid UsuarioId { get; set; }
    [Required, StringLength(200), Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = "";
    [Required, EmailAddress, StringLength(254), Display(Name = "E-mail")]
    public string Email { get; set; } = "";
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 128 caracteres."), DataType(DataType.Password), Display(Name = "Nova senha")]
    public string? Senha { get; set; }
    [EnumDataType(typeof(Role)), Display(Name = "Permissão")]
    public Role Role { get; set; }
}
