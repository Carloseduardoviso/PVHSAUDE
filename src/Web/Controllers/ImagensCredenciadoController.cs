using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class ImagensCredenciadoController(IHttpClientFactory clients) : ControllerBase
{
    [HttpGet("/uploads/credenciados/{nomeArquivo}")]
    public async Task<IActionResult> Obter(string nomeArquivo, CancellationToken ct)
    {
        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        if (Path.GetFileName(nomeArquivo) != nomeArquivo ||
            extensao is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return NotFound();

        try
        {
            using var response = await clients.CreateClient("default")
                .GetAsync($"uploads/credenciados/{Uri.EscapeDataString(nomeArquivo)}", ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return NotFound();
            if (!response.IsSuccessStatusCode) return StatusCode(StatusCodes.Status502BadGateway);

            var tipo = extensao switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
            return File(await response.Content.ReadAsByteArrayAsync(ct), tipo);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }
    }
}
