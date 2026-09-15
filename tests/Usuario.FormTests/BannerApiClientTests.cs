using System.Net;
using System.Net.Http.Json;
using PVHSAUDE.Application.ViewModels;
using Web.Services;

internal static class BannerApiClientTests
{
    public static async Task Run()
    {
        var banner = new BannerVm { Id = Guid.NewGuid(), Titulo = "Banner existente", Ativo = true };
        using var transport = new BannerTransport(banner);
        using var http = new HttpClient(transport) { BaseAddress = new Uri("http://api/") };
        var client = new BannerApiClient(http);
        foreach (var ativos in new[] { false, true })
        {
            var lista = await client.ListarAsync(ativos, default);
            Check(lista.Single().BannerId == banner.Id, "listagem preserva o ID enviado pela API");
        }
        var model = await client.ObterAsync(banner.Id, default);
        Check(model?.BannerId == banner.Id, "abertura da edição preserva o ID enviado pela API");
        model!.Titulo = "Banner editado";
        Check(await client.SalvarAsync(model, default) is null, "edição salva sem reenviar imagem");
        Check(transport.SavedMethod == HttpMethod.Put && transport.SavedPath == $"/api/banners/{banner.Id}",
            "edição atualiza o banner existente com PUT e seu ID original");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Banner: " + message);
        Console.WriteLine("PASS: Banner - " + message);
    }

    private sealed class BannerTransport(BannerVm banner) : HttpMessageHandler
    {
        public HttpMethod? SavedMethod { get; private set; }
        public string? SavedPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var path = request.RequestUri!.AbsolutePath;
            if (request.Method == HttpMethod.Get)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = path == "/api/banners" || path == "/api/banners/ativos"
                        ? JsonContent.Create(new[] { banner }) : JsonContent.Create(banner)
                });
            SavedMethod = request.Method;
            SavedPath = path;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
