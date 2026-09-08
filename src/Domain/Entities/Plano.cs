//using PVHSAUDE.Domain.Enuns;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Plano
//    {
//        public string Nome { get; private set; } = string.Empty;
//        public string? Descricao { get; private set; }
//        public TipoPessoa TipoPessoa { get; private set; }
//        public decimal Valor { get; private set; }
//        public Periodicidade Periodicidade { get; private set; }
//        public int? DiasValidade { get; private set; }
//        public ICollection<Beneficiario> Beneficiarios { get; private set; } = new List<Beneficiario>();
//        public ICollection<Contrato> Contratos { get; private set; } = new List<Contrato>();

//        public Plano(string nome, TipoPessoa tipoPessoa, decimal valor, Periodicidade periodicidade, string? descricao = null)
//        { Nome = nome; TipoPessoa = tipoPessoa; Valor = valor; Periodicidade = periodicidade; Descricao = descricao; }
//    }
//}