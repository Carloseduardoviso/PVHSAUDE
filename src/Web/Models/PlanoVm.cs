using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Domain.Enuns;
using Web.ModelBinders;

namespace Web.Models;

public class PlanoVm
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Informe o nome do plano."), StringLength(150)]
    [Display(Name = "Nome do plano")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Informe um valor válido, maior ou igual a zero.")]
    [ModelBinder(BinderType = typeof(MoedaModelBinder))]
    [Display(Name = "Valor (R$)")]
    public decimal Valor { get; set; }

    [EnumDataType(typeof(Periodicidade))]
    [Display(Name = "Periodicidade")]
    public Periodicidade Periodicidade { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de validade (opcional)")]
    public DateTime? DataValidade { get; set; }
}
