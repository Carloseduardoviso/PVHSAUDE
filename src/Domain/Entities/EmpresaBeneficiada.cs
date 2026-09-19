using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Domain.Entities;

public class EmpresaBeneficiada
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
    public string RazaoSocial { get; private set; } = string.Empty;
    public string NomeFantasia { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? WhatsApp { get; private set; }
    public string? Email { get; private set; }
    public string? Cep { get; private set; }
    public string? Endereco { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }
    public string? Observacoes { get; private set; }
    public TipoCredenciado Tipo { get; private set; }
    public StatusCredenciamento StatusCredenciamento { get; private set; }
    public string? ImagemUrl { get; private set; }
    public ICollection<EmpresaBeneficiadaEspecialidade> Especialidades { get; private set; } = new List<EmpresaBeneficiadaEspecialidade>();
    public ICollection<EmpresaBeneficiadaProcedimento> Procedimentos { get; private set; } = new List<EmpresaBeneficiadaProcedimento>();
    public Guid? PlanoId { get; private set; }
    public Plano? Plano { get; private set; }
    public void DefinirPlano(Guid planoId) => PlanoId = planoId;
    private EmpresaBeneficiada() { }
    public void DefinirImagem(string? imagemUrl) => ImagemUrl = imagemUrl;
    public EmpresaBeneficiada(string razaosocial, string nomefantasia, string cnpj, string? telefone, string? whatsapp, string? email, string? cep, string? endereco, string? cidade, string? uf, string? observacoes, TipoCredenciado tipo, StatusCredenciamento statusCredenciamento) => Atualizar(razaosocial, nomefantasia, cnpj, telefone, whatsapp, email, cep, endereco, cidade, uf, observacoes, tipo, statusCredenciamento);
    public void Atualizar(string razaosocial, string nomefantasia, string cnpj, string? telefone, string? whatsapp, string? email, string? cep, string? endereco, string? cidade, string? uf, string? observacoes, TipoCredenciado tipo, StatusCredenciamento statusCredenciamento)
    {
        RazaoSocial = razaosocial.Trim();
        NomeFantasia = nomefantasia.Trim();
        Cnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        Telefone = telefone?.Trim();
        WhatsApp = whatsapp?.Trim();
        Email = email?.Trim();
        Cep = cep?.Trim();
        Endereco = endereco?.Trim();
        Cidade = cidade?.Trim();
        Uf = uf?.Trim().ToUpperInvariant();
        Observacoes = observacoes?.Trim();
        Tipo = tipo;
        StatusCredenciamento = statusCredenciamento;
    }
}

