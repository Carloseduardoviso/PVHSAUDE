using System.Net;
using System.Net.Http.Json;
using Web.Models;

namespace Web.Services;
public class DescontoApiClient(HttpClient client)
{
    public async Task<IReadOnlyCollection<PlanoVm>> ListarAsync(CancellationToken ct) => await client.GetFromJsonAsync<List<PlanoVm>>("api/descontos", ct) ?? [];
    public async Task<PlanoVm?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var response = await client.GetAsync($"api/descontos/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanoVm>(ct);
    }
    public async Task SalvarAsync(PlanoVm model, CancellationToken ct)
    {
        using var response = model.Id == Guid.Empty ? await client.PostAsJsonAsync("api/descontos", model, ct) : await client.PutAsJsonAsync($"api/descontos/{model.Id}", model, ct);
        response.EnsureSuccessStatusCode();
    }
}
