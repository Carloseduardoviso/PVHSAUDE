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
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("CEP: " + message);
        Console.WriteLine("PASS: CEP - " + message);
    }
}
