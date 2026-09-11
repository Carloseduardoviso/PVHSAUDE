using System.Net;
using Web.Models;

namespace Web.Services;

public class PlanoApiClient(HttpClient client)
{
    public async Task<IReadOnlyCollection<PlanoViewModel>> ListarAsync(CancellationToken ct)
    {
       return await client.GetFromJsonAsync<List<PlanoViewModel>>("api/planos", ct) ?? [];
    }

    public async Task<PlanoViewModel?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var response = await client.GetAsync($"api/planos/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await Verificar(response, ct);
        return await response.Content.ReadFromJsonAsync<PlanoViewModel>(ct);
    }

    public async Task SalvarAsync(PlanoViewModel model, CancellationToken ct)
    {
        using var response = model.Id == Guid.Empty
            ? await client.PostAsJsonAsync("api/planos", model, ct)
            : await client.PutAsJsonAsync($"api/planos/{model.Id}", model, ct);
        await Verificar(response, ct);
    }

    private static async Task Verificar(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        throw new HttpRequestException(await response.Content.ReadAsStringAsync(ct), null, response.StatusCode);
    }
}
