internal static class CepAutofillTests
{
    public static void Run()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Web"));
        var script = File.ReadAllText(Path.Combine(root, "wwwroot", "js", "credenciado-form.js"));

        Check(script.Contains("const locality = cidade?.value") && script.Contains("const state = uf?.value"), "CEP preenche o endereÃ§o mesmo sem campos de cidade e UF");

        Check(!script.Contains("!cidade || !uf"), "CEP não depende de campos de cidade e UF no formulário");
        Check(script.Contains("numero.addEventListener(\"input\", composeAddress)"), "número recompõe o endereço consultado");
        Check(!script.Contains("cidade e UF manualmente"), "mensagens de CEP não orientam campos inexistentes");
        Check(script.Contains("/Administracao/Cep/Consultar"), "CEP é consultado pelo próprio portal");
        Check(!script.Contains("viacep.com.br"), "formulário não depende de acesso externo do navegador");
        Check(script.Contains("window.setTimeout(() => controller.abort(), 50000)"), "browser waits for CEP lookup");
        Check(script.Contains("if (cidade && cidade.value === initialCity) cidade.value = data.cidade;") &&
              script.Contains("if (uf && uf.value === initialUf) uf.value = data.uf;"),
            "CEP does not write city or state fields that are absent from the form");
        var credenciadoForm = File.ReadAllText(Path.Combine(root, "Areas", "Administracao", "Views", "Credenciado", "_Form.cshtml"));
        Check(credenciadoForm.Contains("asp-for=\"PlanoId\"") && credenciadoForm.Contains("Plano vinculado"),
            "edição de credenciamento preserva e mostra o plano já vinculado");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("CEP: " + message);
        Console.WriteLine("PASS: CEP - " + message);
    }
}
