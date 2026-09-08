//using PVHSAUDE.Domain.Enuns;
//using System;
//using System.Collections.Generic;
//using System.Numerics;
//using System.Text;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Contrato
//    {
//        public string Numero { get; private set; } = string.Empty;
//        public int BeneficiarioId { get; private set; }
//        public Beneficiario Beneficiario { get; private set; } = null!;
//        public int PlanoId { get; private set; }
//        public Plano Plano { get; private set; } = null!;
//        public decimal Valor { get; private set; }
//        public Periodicidade Periodicidade { get; private set; }
//        public DateTime DataContratacao { get; private set; }
//        public DateTime? DataCancelamento { get; private set; }
//        public DateTime ProximoVencimento { get; private set; }

//        public Contrato(string numero, int beneficiarioId, int planoId, decimal valor, Periodicidade periodicidade, DateTime contratacao, DateTime proximoVencimento)
//        { 
//            Numero = numero; BeneficiarioId = beneficiarioId; PlanoId = planoId; Valor = valor; Periodicidade = periodicidade; DataContratacao = contratacao; ProximoVencimento = proximoVencimento; 
//        }

//    }
//}