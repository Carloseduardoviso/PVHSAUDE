using System.Net;
using PVHSAUDE.Application.ViewModels;

namespace Web.Services;

public class WhatsAppApiClient(IHttpClientFactory factory)
{
    public async Task<ConfiguracaoWhatsAppVm?> ObterAsync(CancellationToken ct)
    {
        using var client = factory.CreateClient("default");
        using var response = await client.GetAsync("api/configuracao-whatsapp", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ConfiguracaoWhatsAppVm>(ct);
    }

    public async Task SalvarAsync(ConfiguracaoWhatsAppVm model, CancellationToken ct)
    {
        using var client = factory.CreateClient("default");
        using var response = await client.PutAsJsonAsync("api/configuracao-whatsapp", model, ct);
        response.EnsureSuccessStatusCode();
    }
}
