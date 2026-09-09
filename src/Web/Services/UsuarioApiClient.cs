using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using PVHSAUDE.Application.ViewModels;
namespace Web.Services;

public class UsuarioApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    public async Task<LoginResponse?> LoginAsync(LoginVm model, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("Auth/login", model, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
    }
    private async Task<HttpRequestMessage> RequestAsync(HttpMethod method, string path = "api/usuarios")
    {
        var request = new HttpRequestMessage(method, path);
        var token = await accessor.HttpContext!.GetTokenAsync("access_token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
    public async Task<List<UsuarioVm>> ListarAsync(CancellationToken ct)
    {
        using var request = await RequestAsync(HttpMethod.Get);
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<UsuarioVm>>(ct) ?? [];
    }
    public async Task<bool> CriarAsync(UsuarioCadastroVm model, CancellationToken ct)
    {
        using var request = await RequestAsync(HttpMethod.Post);
        request.Content = JsonContent.Create(model);
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<UsuarioVm?> ObterAsync(Guid id, CancellationToken ct)
    {
        using var request = await RequestAsync(HttpMethod.Get, $"api/usuarios/{id}");
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UsuarioVm>(ct);
    }

    public async Task<string?> AlterarAsync(Guid id, UsuarioEdicaoVm? model, string acao, CancellationToken ct)
    {
        var method = model is not null ? HttpMethod.Put : acao == "excluir" ? HttpMethod.Delete : HttpMethod.Post;
        var path = $"api/usuarios/{id}" + (model is null && acao != "excluir" ? $"/{acao}" : "");
        using var request = await RequestAsync(method, path);
        if (model is not null) request.Content = JsonContent.Create(model);
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return "Usuário não encontrado.";
        if (response.StatusCode == HttpStatusCode.Conflict)
            return (await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(ct))?.Detail ?? "Não foi possível alterar o usuário.";
        response.EnsureSuccessStatusCode();
        return null;
    }
}
