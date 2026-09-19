using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Validation;
namespace Web.Models;

public class CredenciadoVm
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }

    [Display(Name = "Desconto")]
    public Guid? DescontoId { get; set; }

    [Display(Name = "Plano")]
    public Guid? PlanoId { get; set; }

    public List<Guid> EspecialidadeIds { get; set; } = [];
    public List<Guid> ProcedimentoIds { get; set; } = [];
    public string? ImagemUrl { get; set; }
    public List<string> ImagemUrls { get; set; } = [];

    [Display(Name = "Imagem da empresa")] 
    public IFormFile? Imagem { get; set; }
    public List<IFormFile> Imagens { get; set; } = [];

    [Required(ErrorMessage = "Informe razão social."), StringLength(150)]
    [Display(Name = "Razão social")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe nome fantasia."), StringLength(150)]
    [Display(Name = "Nome fantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe cnpj."), StringLength(18), RegularExpression(@"(?:\d{14}|\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2})", ErrorMessage = "Informe um CNPJ completo.")]
    [Display(Name = "CNPJ")]
    public string Cnpj { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [StringLength(20)]
    [Display(Name = "WhatsApp")]
    public string? WhatsApp { get; set; }

    [StringLength(254), EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(9), RegularExpression(@"\d{5}-?\d{3}", ErrorMessage = "Informe um CEP completo.")]
    [Display(Name = "CEP")]
    public string? Cep { get; set; }

    [StringLength(250)]
    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [StringLength(100)]
    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [StringLength(2), RegularExpression(@"[A-Za-z]{2}", ErrorMessage = "Informe a sigla da UF.")]
    [Display(Name = "UF")]
    public string? Uf { get; set; }

    [StringLength(1000)]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    [Required(ErrorMessage = "Selecione o menu."), EnumDataType(typeof(TipoCredenciado))]
    [Display(Name = "Menus")]
    public TipoCredenciado? Tipo { get; set; }

    [Required, EnumDataType(typeof(StatusCredenciamento))]
    [Display(Name = "Situação")]
    public StatusCredenciamento? StatusCredenciamento { get; set; } = PVHSAUDE.Domain.Enuns.StatusCredenciamento.Pendente;
}

