using System.Net.Http.Json;

namespace Web.Services;

public sealed class CepConsultaClient(HttpClient client)
{
    public async Task<CepEndereco?> ConsultarAsync(string? cep, CancellationToken ct)
    {
        var digitos = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 8) return null;

        using var response = await client.GetAsync($"https://viacep.com.br/ws/{digitos}/json/", ct);
        if (!response.IsSuccessStatusCode) return null;

        var resposta = await response.Content.ReadFromJsonAsync<RespostaViaCep>(ct);
        if (resposta is null || resposta.Erro || string.IsNullOrWhiteSpace(resposta.Logradouro) ||
            string.IsNullOrWhiteSpace(resposta.Localidade) || string.IsNullOrWhiteSpace(resposta.Uf)) return null;

        return new CepEndereco(resposta.Logradouro, resposta.Bairro, resposta.Localidade, resposta.Uf);
    }

    private sealed record RespostaViaCep(string? Logradouro, string? Bairro, string? Localidade, string? Uf, bool Erro);
}

public sealed record CepEndereco(string Logradouro, string? Bairro, string Cidade, string Uf);
