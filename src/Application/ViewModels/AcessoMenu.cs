using System.Security.Claims;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;

public static class AcessoMenu
{
    public const string Claim = "menu";
    public static bool PodeAcessar(ClaimsPrincipal user, string menu) =>
        user.Identity?.IsAuthenticated == true && (user.IsInRole(nameof(Role.Administrador)) || user.HasClaim(Claim, menu));

    public static void Atualizar(ClaimsIdentity identity, IEnumerable<string> menus)
    {
        foreach (var claim in identity.FindAll(Claim).ToList()) identity.RemoveClaim(claim);
        foreach (var menu in menus.Where(MenusAdministrativos.Opcoes.ContainsKey).Distinct())
            identity.AddClaim(new Claim(Claim, menu));
    }
}
