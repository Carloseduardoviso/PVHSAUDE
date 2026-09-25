using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao"), Authorize(Roles = "Administrador")]
public class AcessoBeneficiarioController(AcessoBeneficiarioApiClient acessos, BeneficiarioApiClient beneficiarios) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(string cpf, CancellationToken ct)
    {
        try
        {
            var emitido = await acessos.EmitirAsync(cpf, ct);
            TempData["AcessoEmitido"] = $"{emitido.Nome} · CPF {emitido.Cpf} · senha temporária: {emitido.SenhaTemporaria}";
        }
        catch (HttpRequestException)
        { TempData["Error"] = "CPF não encontrado entre titulares e dependentes, ou não foi possível emitir o acesso."; }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var pendentes = await acessos.PendentesAsync(ct);
            var titulares = await beneficiarios.ListarAsync(ct);
            return View(new AcessosBeneficiarioVm(pendentes, titulares));
        }
        catch (HttpRequestException)
        {
            ViewData["Erro"] = "Não foi possível consultar as solicitações de acesso.";
            return View(new AcessosBeneficiarioVm([], []));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(Guid usuarioId, Guid beneficiarioId, CancellationToken ct)
    {
        try
        {
            await acessos.AprovarAsync(usuarioId, beneficiarioId, ct);
            TempData["Success"] = "Acesso liberado para o beneficiário.";
        }
        catch (HttpRequestException)
        { TempData["Error"] = "Não foi possível aprovar. Confira se o CPF corresponde ao beneficiário e se ele já possui acesso."; }
        return RedirectToAction(nameof(Index));
    }
}
