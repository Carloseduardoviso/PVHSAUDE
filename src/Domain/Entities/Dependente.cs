using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Domain.Entities
{
    public class Dependente
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Codigo { get; private set; } = string.Empty;
        public Guid BeneficiarioId { get; private set; }
        public Beneficiario Beneficiario { get; private set; } = null!;
        public string Nome { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public DateTime DataNascimento { get; private set; }
        public GrauParentesco GrauParentesco { get; private set; }

        private Dependente() { }

        public void Atualizar(string nome, string cpf, DateTime dataNascimento, GrauParentesco grauParentesco)
        {
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            GrauParentesco = grauParentesco;
        }

        public void DefinirCodigo(string codigo) => Codigo = codigo;

        public Dependente(Guid beneficiarioId, string nome, string cpf, DateTime dataNascimento, GrauParentesco grauParentesco)
        {
            BeneficiarioId = beneficiarioId;
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            GrauParentesco = grauParentesco;
        }
    }
}
