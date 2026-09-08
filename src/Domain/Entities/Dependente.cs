namespace PVHSAUDE.Domain.Entities
{
    public class Dependente
    {
        public int Id { get; private set; }
        public int BeneficiarioId { get; private set; }
        public Beneficiario Beneficiario { get; private set; } = null!;
        public string Nome { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public DateTime DataNascimento { get; private set; }
        public string GrauParentesco { get; private set; } = string.Empty;

        private Dependente() { }

        public Dependente(int beneficiarioId, string nome, string cpf, DateTime dataNascimento, string grauParentesco)
        {
            BeneficiarioId = beneficiarioId;
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            GrauParentesco = grauParentesco;
        }
    }
}
