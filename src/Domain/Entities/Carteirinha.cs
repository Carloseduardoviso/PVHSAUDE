//using PVHSAUDE.Domain.Enuns;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Carteirinha
//    {
//        public string Numero { get; private set; } = string.Empty;
//        public string? QrCode { get; private set; }
//        public int BeneficiarioId { get; private set; }
//        public Beneficiario Beneficiario { get; private set; } = null!;
//        public DateTime DataEmissao { get; private set; }
//        public DateTime DataValidade { get; private set; }
//        public StatusCarteirinha Status { get; private set; } = StatusCarteirinha.Ativa;

//        public Carteirinha(string numero, int beneficiarioId, DateTime validade, string? qrCode = null)
//        { 
//            Numero = numero; BeneficiarioId = beneficiarioId; DataEmissao = DateTime.UtcNow; DataValidade = validade; QrCode = qrCode; 
//        }
//    }
//}