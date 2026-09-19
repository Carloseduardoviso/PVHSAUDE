using System.Text.RegularExpressions;

static class ClientScriptTests
{
    public static void Run()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Web"));
        var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "lib" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        var nativeDialogs = new Regex(@"\b(confirm|alert|prompt)\s*\(", RegexOptions.IgnoreCase);
        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            if (nativeDialogs.IsMatch(source))
                throw new Exception($"Diálogo nativo encontrado em {Path.GetRelativePath(root, file)}.");
        }

        var sweetAlertScript = Path.Combine(root, "wwwroot", "js", "sweetalert-confirm.js");
        if (!File.Exists(sweetAlertScript))
            throw new Exception("Script compartilhado de confirmação SweetAlert2 não encontrado.");

        Console.WriteLine("PASS: diálogos administrativos usam SweetAlert2.");
    }
}
