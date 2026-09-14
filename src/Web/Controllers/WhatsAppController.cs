using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using System.ComponentModel.DataAnnotations;
using Web.Services;

namespace PVHSAUDE.Web.Controllers;

public class WhatsAppController(WhatsAppApiClient api) : Controller
{
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var model = await api.ObterAsync(timeout.Token);
            if (model is not null && Validator.TryValidateObject(model, new ValidationContext(model), null, true))
                return Redirect(model.CriarLink());
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException || ex is OperationCanceledException && !ct.IsCancellationRequested) { }
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("Indisponivel");
    }
}
