using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;

public class MenusValidosAttribute : ValidationAttribute
{
    public MenusValidosAttribute() => ErrorMessage = "Selecione apenas os menus disponíveis.";
    public override bool IsValid(object? value) =>
        value is string[] menus && menus.Length <= MenusAdministrativos.Opcoes.Count && menus.All(x => x is not null && MenusAdministrativos.Opcoes.ContainsKey(x));
}
