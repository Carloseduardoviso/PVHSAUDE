using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Domain.Entities
{
    public class Beneficiario
    {
        public int Id { get; private set; }
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
        public int PlanoId { get; private set; }
        public ICollection<Dependente> Dependentes { get; private set; } = new List<Dependente>();

        public Beneficiario(string nome, string cpf, DateTime dataNascimento, int planoId, DateTime dataInicio, DateTime dataValidade)
        {
            Nome = nome; Cpf = cpf; DataNascimento = dataNascimento; PlanoId = planoId; DataAdesao = DateTime.UtcNow; DataInicio = dataInicio; DataValidade = dataValidade;
        }

        public void Atualizar(string nome, string cpf, DateTime nascimento, string? telefone, string? email, string? endereco, int planoId, DateTime inicio, DateTime validade, StatusBeneficiario status)
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
        }

        public void DefinirStatus(StatusBeneficiario status) => Status = status;
    }
}
