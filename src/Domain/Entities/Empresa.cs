//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PVHSAUDE.Domain.Entities
//{
//    public class Empresa
//    {
//        public string RazaoSocial { get; private set; } = string.Empty;
//        public string NomeFantasia { get; private set; } = string.Empty;
//        public string Cnpj { get; private set; } = string.Empty;
//        public string? Telefone { get; private set; }
//        public string? Email { get; private set; }
//        public Endereco? Endereco { get; private set; }
//        public ICollection<Beneficiario> Beneficiarios { get; private set; } = new List<Beneficiario>();

//        public Empresa(string razaoSocial, string nomeFantasia, string cnpj)
//        {
//            RazaoSocial = razaoSocial; NomeFantasia = nomeFantasia; Cnpj = cnpj;
//        }
//    }
//}