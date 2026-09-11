using System.Net;
using Web.Models;

namespace Web.Services;

public class BeneficiarioApiClient(HttpClient httpClient) 
{
    public async Task<IReadOnlyCollection<BeneficiarioViewModel>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<BeneficiarioViewModel>>("api/beneficiarios", cancellationToken) ?? [];
    }

    public async Task<BeneficiarioViewModel?> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/beneficiarios/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await GarantirSucesso(response);

        return await response.Content.ReadFromJsonAsync<BeneficiarioViewModel>(cancellationToken);
    }

    public async Task CriarAsync(BeneficiarioViewModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/beneficiarios", model, cancellationToken);
        await GarantirSucesso(response);
    }

    public async Task AtualizarAsync(BeneficiarioViewModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/beneficiarios/{model.Id}", model, cancellationToken);
        await GarantirSucesso(response);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/beneficiarios/{id}", cancellationToken);
        await GarantirSucesso(response);
    }

    public async Task InativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync($"api/beneficiarios/{id}/inativar", null, cancellationToken);
        await GarantirSucesso(response);
    }

    private static async Task GarantirSucesso(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var detalhe = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(string.IsNullOrWhiteSpace(detalhe) ? "Não foi possível concluir a operação na API." : detalhe, null, response.StatusCode);
    }
}
