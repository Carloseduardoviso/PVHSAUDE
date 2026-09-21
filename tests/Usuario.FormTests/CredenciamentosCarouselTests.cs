internal static class CredenciamentosCarouselTests
{
    public static void Run()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Web"));
        var controller = File.ReadAllText(Path.Combine(root, "Controllers", "HomeController.cs"));
        var banners = File.ReadAllText(Path.Combine(root, "Views", "Home", "_Banners.cshtml"));
        var carousel = Path.Combine(root, "Views", "Home", "_CredenciamentosCarousel.cshtml");
        var carouselView = File.Exists(carousel) ? File.ReadAllText(carousel) : string.Empty;
        var script = File.ReadAllText(Path.Combine(root, "wwwroot", "js", "portal-carousel.js"));
        var portalVm = File.ReadAllText(Path.Combine(root, "Models", "PortalVm.cs"));

        Check(controller.Contains("CarregarCredenciamentosAtivos"), "início carrega credenciamentos ativos");
        Check(banners.Contains("_CredenciamentosCarousel"), "início inclui o carrossel de credenciamentos");
        Check(File.Exists(carousel), "carrossel de credenciamentos possui view própria");
        Check(!carouselView.Contains("Ver todos"), "carrossel não exibe atalho para ver todos");
        Check(!carouselView.Contains(".Chunk(3)"), "cada credenciamento ocupa seu próprio slide");
        Check(carouselView.Contains("data-always-controls"), "carrossel solicita setas mesmo com um cadastro");
        Check(script.Contains("data-always-controls"), "controles permanentes são tratados pelo script");
        Check(carouselView.Contains("Desconto") && carouselView.Contains("Nome fantasia") && carouselView.Contains("CNPJ"), "carrossel exibe os dados comerciais da empresa");
        Check(carouselView.Contains("WhatsApp") && carouselView.Contains("Endereço"), "carrossel exibe os dados de contato da empresa");
        Check(carouselView.Contains("Especialidades") && carouselView.Contains("Procedimentos"), "carrossel exibe os serviços cadastrados");
        Check(!carouselView.Contains("Não informado") && !carouselView.Contains("Não informadas") && !carouselView.Contains("Não informados"), "carrossel oculta campos sem dados cadastrados");
        Check(portalVm.Contains("Descontos"), "portal disponibiliza os descontos para o carrossel");
        Check(controller.Contains("EspecialidadesAsync") && controller.Contains("ProcedimentosAsync") && controller.Contains("descontos.ListarAsync"), "início carrega os catálogos necessários ao carrossel");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Credenciamentos: " + message);
        Console.WriteLine("PASS: Credenciamentos - " + message);
    }
}
