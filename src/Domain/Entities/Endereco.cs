//namespace PVHSAUDE.Domain.Entities
//{
//    public class Endereco
//    {
//        public string Cep { get; private set; } = string.Empty;
//        public string Logradouro { get; private set; } = string.Empty;
//        public string Numero { get; private set; } = string.Empty;
//        public string? Complemento { get; private set; }
//        public string Bairro { get; private set; } = string.Empty;
//        public string Cidade { get; private set; } = string.Empty;
//        public string Uf { get; private set; } = string.Empty;
//        public decimal? Latitude { get; private set; }
//        public decimal? Longitude { get; private set; }

//        private Endereco() { }

//        public Endereco(string cep, string logradouro, string numero, string bairro, string cidade, string uf,
//            string? complemento = null, decimal? latitude = null, decimal? longitude = null)
//        {
//            Cep = cep; Logradouro = logradouro; Numero = numero; Bairro = bairro; Cidade = cidade; Uf = uf;
//            Complemento = complemento; Latitude = latitude; Longitude = longitude;
//        }
//    }
//}