namespace PVHSAUDE.Domain.Enuns;

public static class MenusAdministrativos
{
    // Cadastro único dos menus administrativos. Ao adicionar uma entrada aqui,
    // ela passa automaticamente a aparecer no formulário de usuários e no menu lateral.
    public static readonly IReadOnlyDictionary<string, string> Opcoes = new Dictionary<string, string>
    {
        ["Banner"] = "Banner",
        ["WhatsApp"] = "WhatsApp",
        ["Contato"] = "Mensagens",
        ["Beneficiario"] = "Beneficiários",
        ["Plano"] = "Planos",
        ["Credenciado"] = "Clínicas Credenciadas",
        ["EmpresaBeneficiada"] = "Empresas Beneficiadas",
        ["Especialidades"] = "Especialidades",
        ["Procedimentos"] = "Procedimentos"
    };
    public static string[] Ler(string valor) => valor.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Where(Opcoes.ContainsKey).Distinct().ToArray();
    public static string Gravar(IEnumerable<string> menus) => string.Join(",", menus.Distinct().Order());
}
