namespace PVHSAUDE.Domain.Entities;

public class Procedimento
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;
    public ICollection<CredenciadoProcedimento> Credenciados { get; private set; } = new List<CredenciadoProcedimento>();
    private Procedimento() { }
    public Procedimento(string nome) => Nome = nome.Trim();
    public void Atualizar(string nome, bool ativo) { Nome = nome.Trim(); Ativo = ativo; }
}
