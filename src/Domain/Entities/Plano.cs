using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Domain.Entities;

public class Plano
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public Periodicidade Periodicidade { get; private set; }
    public DateTime? DataValidade { get; private set; }

    public TipoPessoa TipoPessoa { get; private set; }

    private Plano() { }

    public Plano(string nome, string? descricao, decimal valor, Periodicidade periodicidade, DateTime? dataValidade, TipoPessoa tipoPessoa = TipoPessoa.Fisica)
        => Atualizar(nome, descricao, valor, periodicidade, dataValidade, tipoPessoa);

    public void Atualizar(string nome, string? descricao, decimal valor, Periodicidade periodicidade, DateTime? dataValidade, TipoPessoa tipoPessoa = TipoPessoa.Fisica)
    {
        if (!Enum.IsDefined(tipoPessoa)) throw new ArgumentOutOfRangeException(nameof(tipoPessoa));
        TipoPessoa = tipoPessoa;
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        Valor = valor;
        Periodicidade = periodicidade;
        DataValidade = dataValidade?.Date;
    }
}
