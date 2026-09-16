using System.Net;
using System.Net.Http.Json;
using Web.Models;
using PVHSAUDE.Domain.Enuns;

namespace Web.Services;

public class IntencaoVendaApiClient(HttpClient http)
{
    public async Task CriarAsync(IntencaoVendaEntradaVm vm, CancellationToken ct) { using var r = await http.PostAsJsonAsync("api/intencoes-venda", vm, ct); r.EnsureSuccessStatusCode(); }
    public async Task<List<IntencaoVendaVm>> ListarAsync(CancellationToken ct) => await http.GetFromJsonAsync<List<IntencaoVendaVm>>("api/intencoes-venda", ct) ?? [];
    public async Task AtualizarStatusAsync(Guid id, StatusIntencaoVenda status, CancellationToken ct) { using var r = await http.PutAsJsonAsync($"api/intencoes-venda/{id}/status", status, ct); r.EnsureSuccessStatusCode(); }
}
