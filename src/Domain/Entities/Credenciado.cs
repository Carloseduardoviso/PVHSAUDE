//using PVHSAUDE.Domain.Enuns;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Credenciado
//    {
//        public string Nome { get; private set; } = string.Empty;
//        public string? RazaoSocial { get; private set; }
//        public string? Cnpj { get; private set; }
//        public string? Telefone { get; private set; }
//        public string? WhatsApp { get; private set; }
//        public TipoCredenciado Tipo { get; private set; }
//        public StatusCredenciamento StatusCredenciamento { get; private set; } = StatusCredenciamento.Pendente;
//        public string? Observacoes { get; private set; }
//        public Endereco? Endereco { get; private set; }
//        public ICollection<Especialidade> Especialidades { get; private set; } = new List<Especialidade>();
//        public ICollection<Procedimento> Procedimentos { get; private set; } = new List<Procedimento>();

//        public Credenciado(string nome, TipoCredenciado tipo) { Nome = nome; Tipo = tipo; }
//    }
//}