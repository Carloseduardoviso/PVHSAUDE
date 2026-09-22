using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using PVHSAUDE.Api.Services;

internal static class ImagemStorageTests
{
    public static async Task Run()
    {
        var raizTemporaria = Path.GetFullPath(Path.GetTempPath());
        var raiz = Path.Combine(raizTemporaria, "pvhsaude-imagens-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(raiz);
        try
        {
            var storage = new ImagemStorage(new TestEnvironment(raiz));
            var url = await storage.SalvarCredenciadoAsync(Guid.NewGuid(), ".png", new MemoryStream([1, 2, 3]), CancellationToken.None);
            var arquivo = Path.Combine(raiz, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(arquivo)) throw new Exception("A imagem não foi salva na pasta de uploads.");

            await storage.ExcluirCredenciadoAsync(url, CancellationToken.None);
            if (File.Exists(arquivo)) throw new Exception("A imagem removida ainda existe na pasta de uploads.");

            var fora = Path.Combine(raiz, "fora.png");
            File.WriteAllBytes(fora, [4, 5, 6]);
            await storage.ExcluirCredenciadoAsync("/uploads/credenciados/..\\fora.png", CancellationToken.None);
            await storage.ExcluirCredenciadoAsync("/uploads/credenciados/C:fora.png", CancellationToken.None);
            if (!File.Exists(fora)) throw new Exception("A exclusão alcançou um arquivo fora da pasta de uploads.");
            Console.WriteLine("PASS: armazenamento exclui somente imagens da pasta de uploads.");
        }
        finally
        {
            if (raiz.StartsWith(raizTemporaria.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                Directory.Delete(raiz, recursive: true);
        }
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "PVHSAUDE.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
