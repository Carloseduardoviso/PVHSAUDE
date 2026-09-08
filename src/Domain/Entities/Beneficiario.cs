using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Domain.Entities
{
    public class Beneficiario
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Nome { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public DateTime DataNascimento { get; private set; }
        public string? Telefone { get; private set; }
        public string? Email { get; private set; }
        public string? Endereco { get; private set; }
        public StatusBeneficiario Status { get; private set; } = StatusBeneficiario.Pendente;
        public TipoBeneficiario Tipo { get; private set; } = TipoBeneficiario.Titular;
        public DateTime DataAdesao { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataValidade { get; private set; }
        public Guid PlanoId { get; private set; }
        public Guid? CredenciadoId { get; private set; }
        public Credenciado? Credenciado { get; private set; }
        public ICollection<Dependente> Dependentes { get; private set; } = new List<Dependente>();

        public Beneficiario(string nome, string cpf, DateTime dataNascimento, Guid planoId, DateTime dataInicio, DateTime dataValidade, Guid? credenciadoId = null)
        {
            Nome = nome; Cpf = cpf; DataNascimento = dataNascimento; PlanoId = planoId; DataAdesao = DateTime.UtcNow; DataInicio = dataInicio; DataValidade = dataValidade; CredenciadoId = credenciadoId;
        }

        public void Atualizar(string nome, string cpf, DateTime nascimento, string? telefone, string? email, string? endereco, Guid planoId, DateTime inicio, DateTime validade, StatusBeneficiario status, Guid? credenciadoId = null)
        {
            Nome = nome;
            Cpf = cpf;
            DataNascimento = nascimento;
            Telefone = telefone;
            Email = email;
            Endereco = endereco;
            PlanoId = planoId;
            DataInicio = inicio;
            DataValidade = validade;
            Status = status;
            CredenciadoId = credenciadoId;
        }

        public void DefinirStatus(StatusBeneficiario status) => Status = status;
    }
}

