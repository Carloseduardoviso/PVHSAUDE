using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Domain.Entities;

public class IntencaoVenda
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string Telefone { get; private set; } = "";
    public string Cpf { get; private set; } = "";
    public Guid PlanoId { get; private set; }
    public string PlanoNome { get; private set; } = "";
    public decimal ValorPlano { get; private set; }
    public int QuantidadeDependentes { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string Endereco { get; private set; } = "";
    public string Dependentes { get; private set; } = "";
    public StatusIntencaoVenda Status { get; private set; } = StatusIntencaoVenda.AguardandoPagamento;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private IntencaoVenda() { }

    public IntencaoVenda(string nome, string email, string telefone, string cpf, Guid planoId, string planoNome,
        decimal valorPlano, int quantidadeDependentes, string endereco, string dependentes)
    {
        Nome = nome.Trim(); Email = email.Trim(); Telefone = telefone.Trim(); Cpf = cpf.Trim(); PlanoId = planoId;
        PlanoNome = planoNome.Trim(); ValorPlano = valorPlano; QuantidadeDependentes = quantidadeDependentes;
        ValorTotal = valorPlano + quantidadeDependentes * 11.50m; Endereco = endereco.Trim(); Dependentes = dependentes.Trim();
    }

    public void AtualizarStatus(StatusIntencaoVenda status) => Status = status;
}
