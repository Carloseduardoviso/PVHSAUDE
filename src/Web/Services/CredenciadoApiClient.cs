using System.Net;
using Web.Models;

namespace Web.Services;

public class CredenciadoApiClient(HttpClient client)
{
    public async Task<IReadOnlyCollection<CatalogoItemVm>> EspecialidadesAsync(CancellationToken ct)
    {
        return await client.GetFromJsonAsync<List<CatalogoItemVm>>("api/especialidades", ct) ?? [];
    }

    public async Task<IReadOnlyCollection<CatalogoItemVm>> ProcedimentosAsync(CancellationToken ct)
    {
        return await client.GetFromJsonAsync<List<CatalogoItemVm>>("api/procedimentos", ct) ?? [];
    }

    public async Task<IReadOnlyCollection<CredenciadoVm>> ListarAsync(CancellationToken ct)
    {
        return await client.GetFromJsonAsync<List<CredenciadoVm>>("api/credenciados", ct) ?? [];
    }

    public async Task<CredenciadoVm?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var response = await client.GetAsync($"api/credenciados/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await Verificar(response, ct);

        return await response.Content.ReadFromJsonAsync<CredenciadoVm>(ct);
    }

    public async Task SalvarAsync(CredenciadoVm model, CancellationToken ct)
    {
        using var response = model.Id == Guid.Empty
            ? await client.PostAsJsonAsync("api/credenciados", model, ct)
            : await client.PutAsJsonAsync($"api/credenciados/{model.Id}", model, ct);
        await Verificar(response, ct);
    }

    public async Task UploadImagemAsync(Guid id, IFormFile imagem, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = imagem.OpenReadStream();
        using var file = new StreamContent(stream);

        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imagem.ContentType);
        content.Add(file, "imagem", imagem.FileName);

        using var response = await client.PostAsync($"api/credenciados/{id}/imagem", content, ct);

        await Verificar(response, ct);
    }

    private static async Task Verificar(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        throw new HttpRequestException(await response.Content.ReadAsStringAsync(ct), null, response.StatusCode);
    }
}