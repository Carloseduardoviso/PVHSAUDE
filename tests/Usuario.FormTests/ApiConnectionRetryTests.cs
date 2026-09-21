using System.Net;
using System.Net.Sockets;
using Web.Services;

internal static class ApiConnectionRetryTests
{
    public static async Task Run()
    {
        var transport = new UnavailableThenAvailableHandler(2);
        using var client = new HttpClient(new ApiConnectionRetryHandler(TimeSpan.Zero, 3)
        {
            InnerHandler = transport
        });

        using var response = await client.GetAsync("https://api.test/health");
        if (response.StatusCode != HttpStatusCode.OK || transport.Attempts != 3)
            throw new Exception("A Web deve aguardar a API ficar disponível antes de falhar.");

        Console.WriteLine("PASS: conexão da API é repetida durante a inicialização.");
    }

    private sealed class UnavailableThenAvailableHandler(int failures) : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Attempts++;
            if (Attempts <= failures)
                throw new HttpRequestException("API ainda iniciando.", new SocketException((int)SocketError.ConnectionRefused));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
