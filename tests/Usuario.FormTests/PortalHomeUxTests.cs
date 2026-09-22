internal static class PortalHomeUxTests
{
    public static void Run()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Web"));
        var css = File.ReadAllText(Path.Combine(root, "wwwroot", "css", "site.css"));
        var script = File.ReadAllText(Path.Combine(root, "wwwroot", "js", "portal-carousel.js"));
        var bannerScript = File.ReadAllText(Path.Combine(root, "wwwroot", "js", "banner-form.js"));
        var bannersView = File.ReadAllText(Path.Combine(root, "Views", "Home", "_Banners.cshtml"));
        var sobreNos = File.ReadAllText(Path.Combine(root, "Views", "Home", "SobreNos.cshtml"));

        Check(css.Contains(".portal-banner-layout .banner-lateral-esquerda .banner-imagem") && css.Contains("height: 100%; object-fit: contain;"), "banners laterais preservam a arte sem corte");
        Check(css.Contains(".banner-central { order: -1;"), "banner central tem prioridade em telas menores");
        Check(script.Contains("interval: 8000"), "carrossel aguarda oito segundos antes de trocar");
        Check(bannerScript.Contains("Uso recomendado:"), "cadastro explica a função de cada posição");
        Check(bannersView.Contains("h6 visually-hidden banner-titulo"), "títulos técnicos ficam ocultos visualmente no portal");
        Check(sobreNos.Contains("sobre-nos-hero") && sobreNos.Contains("sobre-nos-cuidado-familiar.png"), "sobre nós apresenta hero com imagem institucional");
        Check(sobreNos.Contains("Nossa história") && sobreNos.Contains("Grupo Kaheli"), "sobre nós explica a origem da rede");
        Check(sobreNos.Contains("Terapias essenciais") && sobreNos.Contains("Saúde em rede") && sobreNos.Contains("Economia no dia a dia"), "sobre nós apresenta os benefícios principais");
        Check(sobreNos.Contains("descontos de até 40%") && sobreNos.Contains("Sua saúde começa aqui."), "sobre nós apresenta economia e chamada final");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Portal UX: " + message);
        Console.WriteLine("PASS: Portal UX - " + message);
    }
}
