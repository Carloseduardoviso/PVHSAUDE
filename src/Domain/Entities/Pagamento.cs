//using PVHSAUDE.Domain.Enuns;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Pagamento
//    {
//        public int BeneficiarioId { get; private set; }
//        public Beneficiario Beneficiario { get; private set; } = null!;
//        public int? ContratoId { get; private set; }
//        public Contrato? Contrato { get; private set; }
//        public decimal Valor { get; private set; }
//        public DateTime DataVencimento { get; private set; }
//        public DateTime? DataPagamento { get; private set; }
//        public StatusPagamento Status { get; private set; } = StatusPagamento.Pendente;
//        public string? FormaPagamento { get; private set; }
//        public string? Observacao { get; private set; }

//        public Pagamento(int beneficiarioId, decimal valor, DateTime vencimento)
//        {
//            BeneficiarioId = beneficiarioId; Valor = valor; DataVencimento = vencimento;
//        }

//        public void RegistrarPagamento(DateTime data, string? forma = null)
//        {
//            DataPagamento = data; FormaPagamento = forma; Status = StatusPagamento.Pago;
//        }
//    }
//}
