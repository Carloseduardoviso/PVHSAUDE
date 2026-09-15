using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Application.ViewModels;

public class PlanoEntradaVm
{
    [EnumDataType(typeof(TipoPessoa)), Display(Name = "Tipo de pessoa")]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Fisica;

    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [StringLength(1000)] public string? Descricao { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true)] public decimal Valor { get; set; }
    [EnumDataType(typeof(Periodicidade))] public Periodicidade Periodicidade { get; set; }
    [DataType(DataType.Date)] public DateTime? DataValidade { get; set; }
}

public record PlanoRespostaVm(Guid Id, string Nome, string? Descricao, decimal Valor, Periodicidade Periodicidade, DateTime? DataValidade, TipoPessoa TipoPessoa = TipoPessoa.Fisica);
