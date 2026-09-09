using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Api.Contracts;

public class CredenciadoRequest
{
    [PVHSAUDE.Domain.Validation.GuidNaoVazio(ErrorMessage = "Selecione o plano da empresa.")]
    public Guid? PlanoId { get; set; }
    [Required(ErrorMessage = "Informe razão social."), StringLength(150), Display(Name = "Razão social")]
    public string RazaoSocial { get; set; } = string.Empty;
    [Required(ErrorMessage = "Informe nome fantasia."), StringLength(150), Display(Name = "Nome fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;
    [Required(ErrorMessage = "Informe cnpj."), StringLength(18), Display(Name = "CNPJ"), RegularExpression(@"(?:\d{14}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})", ErrorMessage = "Informe um CNPJ completo.")]
    public string Cnpj { get; set; } = string.Empty;
    [StringLength(20), Display(Name = "Telefone")]
    public string? Telefone { get; set; }
    [StringLength(20), Display(Name = "WhatsApp")]
    public string? WhatsApp { get; set; }
    [StringLength(254), Display(Name = "E-mail"), EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string? Email { get; set; }
    [StringLength(9), Display(Name = "CEP"), RegularExpression(@"\d{5}-?\d{3}", ErrorMessage = "Informe um CEP completo.")]
    public string? Cep { get; set; }
    [StringLength(250), Display(Name = "Endereço")]
    public string? Endereco { get; set; }
    [StringLength(100), Display(Name = "Cidade")]
    public string? Cidade { get; set; }
    [StringLength(2), Display(Name = "UF"), RegularExpression(@"[A-Za-z]{2}", ErrorMessage = "Informe a sigla da UF.")]
    public string? Uf { get; set; }
    [StringLength(1000), Display(Name = "Observações")]
    public string? Observacoes { get; set; }
    [Required(ErrorMessage = "Selecione o tipo de estabelecimento."), EnumDataType(typeof(TipoCredenciado)), Display(Name = "Tipo de estabelecimento")]
    public TipoCredenciado? Tipo { get; set; }
    public List<Guid> EspecialidadeIds { get; set; } = [];
    public List<Guid> ProcedimentoIds { get; set; } = [];
    [Required, EnumDataType(typeof(StatusCredenciamento)), Display(Name = "Situação")]
    public StatusCredenciamento? StatusCredenciamento { get; set; } = PVHSAUDE.Domain.Enuns.StatusCredenciamento.Pendente;
}
public record CredenciadoResponse(Guid Id, string RazaoSocial, string NomeFantasia, string Cnpj, string? Telefone, string? WhatsApp, string? Email, string? Cep, string? Endereco, string? Cidade, string? Uf, string? Observacoes, TipoCredenciado Tipo, StatusCredenciamento StatusCredenciamento, string? ImagemUrl, List<Guid> EspecialidadeIds, List<Guid> ProcedimentoIds, Guid? PlanoId);
