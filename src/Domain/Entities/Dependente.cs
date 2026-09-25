using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Domain.Entities
{
    public class Dependente
    {
        public const decimal Valor = 11.90m;
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Codigo { get; private set; } = string.Empty;
        public Guid BeneficiarioId { get; private set; }
        public Beneficiario Beneficiario { get; private set; } = null!;
        public string Nome { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public GrauParentesco GrauParentesco { get; private set; }

        private Dependente() { }

        public void Atualizar(string nome, string cpf, DateTime dataNascimento, GrauParentesco grauParentesco, string? email = null)
        {
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            GrauParentesco = grauParentesco;
            Email = email?.Trim();
        }

        public void DefinirCodigo(string codigo) => Codigo = codigo;

        public Dependente(Guid beneficiarioId, string nome, string cpf, DateTime dataNascimento, GrauParentesco grauParentesco, string? email = null)
        {
            BeneficiarioId = beneficiarioId;
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            GrauParentesco = grauParentesco;
            Email = email?.Trim();
        }
    }
}
