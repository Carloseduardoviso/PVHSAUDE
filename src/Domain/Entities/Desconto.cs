namespace PVHSAUDE.Domain.Entities;

public class Desconto
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    private Desconto() { }
    public Desconto(string nome) => Nome = nome.Trim();
    public void Atualizar(string nome, bool ativo) { Nome = nome.Trim(); Ativo = ativo; }
}
