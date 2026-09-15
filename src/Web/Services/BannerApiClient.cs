using System.Net;
using Web.Models;
namespace Web.Services;

public class BannerApiClient(HttpClient http)
{
    public async Task<List<BannerVm>> ListarAsync(bool ativos, CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<BannerVm>>(ativos ? "api/banners/ativos" : "api/banners", ct) ?? [];
    }

    public async Task<BannerVm?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var response = await http.GetAsync($"api/banners/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BannerVm>(ct);
    }

    public async Task<(byte[] Bytes, string Tipo)?> ImagemAsync(Guid id, CancellationToken ct)
    {
        using var response = await http.GetAsync($"api/banners/{id}/imagem", ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadAsByteArrayAsync(ct), response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
    }

    public async Task<string?> SalvarAsync(BannerVm model, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.Titulo), "Titulo");
        content.Add(new StringContent(model.Ativo.ToString()), "Ativo");

        if (model.Imagem is { } imagem)
            content.Add(new StreamContent(imagem.OpenReadStream()), "Imagem", Path.GetFileName(imagem.FileName));

        using var request = new HttpRequestMessage(model.Id == Guid.Empty ? HttpMethod.Post : HttpMethod.Put, model.Id == Guid.Empty ? "api/banners" : $"api/banners/{model.Id}")
        {
            Content = content
        };

        using var response = await http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.BadRequest) return "Informe o título e envie uma imagem em paisagem na proporção 16:5 (1600 × 500 pixels), JPG, PNG ou WEBP de até 5 MB.";
        response.EnsureSuccessStatusCode();

        return null;
    }
}
