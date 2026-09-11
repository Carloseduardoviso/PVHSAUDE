using System.Net;
using Web.Models;
namespace Web.Services;

public class ContatoApiClient(HttpClient http)
{
    public async Task<List<ContatoViewModel>> ListarAsync(CancellationToken ct)
    {
        return await http.GetFromJsonAsync<List<ContatoViewModel>>("api/contatos", ct) ?? [];
    }

    public async Task EnviarAsync(ContatoViewModel m, CancellationToken ct)
    {
        using var r = await http.PostAsJsonAsync("api/contatos", m, ct);
        r.EnsureSuccessStatusCode();
    }

    public async Task<ContatoViewModel?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var r = await http.GetAsync($"api/contatos/{id}", ct);

        if (r.StatusCode == HttpStatusCode.NotFound) return null;
        r.EnsureSuccessStatusCode();

        return await r.Content.ReadFromJsonAsync<ContatoViewModel>(ct);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        using var r = await http.DeleteAsync($"api/contatos/{id}", ct);
        r.EnsureSuccessStatusCode();
    }
}
