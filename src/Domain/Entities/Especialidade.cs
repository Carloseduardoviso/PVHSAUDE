namespace PVHSAUDE.Domain.Entities;

public class Especialidade
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;
    public ICollection<CredenciadoEspecialidade> Credenciados { get; private set; } = new List<CredenciadoEspecialidade>();
    private Especialidade() { }
    public Especialidade(string nome) => Nome = nome.Trim();
    public void Atualizar(string nome, bool ativo) { Nome = nome.Trim(); Ativo = ativo; }
}
