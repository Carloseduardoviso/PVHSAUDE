using System.Net;
using PVHSAUDE.Application.ViewModels;

namespace Web.Services;

public record ResultadoLoginBeneficiario(LoginResponse? Login, bool SemCadastro);

public class AcessoBeneficiarioApiClient(HttpClient client)
{
    public async Task<HttpStatusCode> RegistrarAsync(CadastroAcessoBeneficiarioVm model, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("api/acesso-beneficiario/cadastro", model, ct);
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict) return response.StatusCode;
        response.EnsureSuccessStatusCode();
        return response.StatusCode;
    }

    public async Task<ResultadoLoginBeneficiario> LoginCpfAsync(LoginCpfBeneficiarioVm model, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("Auth/beneficiario", model, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return new(null, true);
        if (response.StatusCode == HttpStatusCode.Unauthorized) return new(null, false);
        response.EnsureSuccessStatusCode();
        return new(await response.Content.ReadFromJsonAsync<LoginResponse>(ct), false);
    }

    public async Task<AcessoBeneficiarioEmitidoVm> EmitirAsync(string cpf, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("api/acesso-beneficiario/emitir", new { cpf }, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AcessoBeneficiarioEmitidoVm>(ct))!;
    }
    public Task<AreaBeneficiarioVm?> MinhaAreaAsync(CancellationToken ct) =>
        client.GetFromJsonAsync<AreaBeneficiarioVm>("api/acesso-beneficiario/me", ct);

    public async Task<List<AcessoBeneficiarioPendenteVm>> PendentesAsync(CancellationToken ct) =>
        await client.GetFromJsonAsync<List<AcessoBeneficiarioPendenteVm>>("api/acesso-beneficiario/pendentes", ct) ?? [];

    public async Task AprovarAsync(Guid usuarioId, Guid beneficiarioId, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync($"api/acesso-beneficiario/pendentes/{usuarioId}/aprovar", new { beneficiarioId }, ct);
        response.EnsureSuccessStatusCode();
    }
}
