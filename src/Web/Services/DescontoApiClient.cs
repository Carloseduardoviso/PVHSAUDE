using System.Net.Http.Json;
using Web.Models;

namespace Web.Services;
public class DescontoApiClient(HttpClient client)
{
    public async Task<IReadOnlyCollection<CatalogoItemVm>> ListarAsync(CancellationToken ct) => await client.GetFromJsonAsync<List<CatalogoItemVm>>("api/descontos", ct) ?? [];
    public async Task CriarAsync(string nome, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("api/descontos", new { nome, ativo = true }, ct);
        response.EnsureSuccessStatusCode();
    }
    public async Task AtualizarAsync(Guid id, string nome, CancellationToken ct)
    {
        using var response = await client.PutAsJsonAsync($"api/descontos/{id}", new { nome, ativo = true }, ct);
        response.EnsureSuccessStatusCode();
    }
    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        using var response = await client.DeleteAsync($"api/descontos/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
