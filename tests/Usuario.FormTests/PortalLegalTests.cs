internal static class PortalLegalTests
{
    public static void Run()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Web"));
        var layout = File.ReadAllText(Path.Combine(root, "Views", "Shared", "_Layout.cshtml"));
        var css = File.ReadAllText(Path.Combine(root, "wwwroot", "css", "site.css"));
        var controller = File.ReadAllText(Path.Combine(root, "Controllers", "HomeController.cs"));
        var privacidade = File.ReadAllText(Path.Combine(root, "Views", "Home", "PoliticaPrivacidade.cshtml"));
        var termos = File.ReadAllText(Path.Combine(root, "Views", "Home", "TermosDeUso.cshtml"));

        Check(layout.Contains("asp-action=\"PoliticaPrivacidade\"") && layout.Contains("Política de Privacidade"),
            "rodapé exibe o link da Política de Privacidade");
        Check(layout.Contains("asp-action=\"TermosDeUso\"") && layout.Contains("Termos de Uso"),
            "rodapé exibe o link dos Termos de Uso");
        Check(!layout.Contains(">Privacy</a>"), "rodapé não mantém o rótulo antigo em inglês");
        Check(css.Contains(".portal-footer-inner { display: flex;") && css.Contains(".portal-footer-links"),
            "rodapé organiza copyright e links institucionais em duas áreas");
        Check(css.Contains(".legal-page { width: min(100% - 2rem, 880px);") && css.Contains(".legal-hero"),
            "páginas legais usam layout próprio do portal");
        Check(css.Contains(".legal-hero { padding: 1.25rem; border-radius: 1rem; }"),
            "páginas legais se adaptam ao celular");

        Check(controller.Contains("public IActionResult PoliticaPrivacidade()") &&
              controller.Contains("public IActionResult TermosDeUso()"),
            "controller publica as duas páginas legais");
        Check(controller.Contains("View(nameof(PoliticaPrivacidade))"), "endereço antigo /Home/Privacy continua funcionando");

        Check(privacidade.Contains("13.709/2018") && privacidade.Contains("LGPD"), "política cita a LGPD");
        Check(privacidade.Contains("PVHSAUDE.Administracao") && privacidade.Contains("HttpOnly"),
            "política descreve o cookie de sessão realmente usado pelo sistema");
        Check(privacidade.Contains("carteirinha") && privacidade.Contains("ViaCEP") && privacidade.Contains("Pix"),
            "política cobre carteirinha, consulta de CEP e pagamento do sistema");
        Check(privacidade.Contains("art. 18 da LGPD", StringComparison.OrdinalIgnoreCase) &&
              privacidade.Contains("revogação do consentimento", StringComparison.OrdinalIgnoreCase),
            "política lista os direitos do titular");
        Check(privacidade.Contains("Nunca vendemos seus dados"), "política deixa claro que dados não são vendidos");

        Check(termos.Contains("18 anos ou mais") && termos.Contains("Pix"), "termos definem elegibilidade e pagamento via Pix");
        Check(termos.Contains("5 dependentes") && termos.Contains("intenção de venda"),
            "termos descrevem dependentes e o fluxo de solicitação do sistema");
        Check(termos.Contains("não é plano de saúde"), "termos deixam claro que o cartão é de descontos");
        Check(termos.Contains("Política de Privacidade"), "termos apontam para a política de privacidade");
        Check(termos.Contains("Porto Velho/RO"), "termos definem foro");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Portal legal: " + message);
        Console.WriteLine("PASS: Portal legal - " + message);
    }
}
