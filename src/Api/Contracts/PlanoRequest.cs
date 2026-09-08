using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Api.Contracts;

public class PlanoRequest
{
    [Required, StringLength(150)] public string Nome { get; set; } = string.Empty;
    [StringLength(1000)] public string? Descricao { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true)] public decimal Valor { get; set; }
    [EnumDataType(typeof(Periodicidade))] public Periodicidade Periodicidade { get; set; }
    [DataType(DataType.Date)] public DateTime? DataValidade { get; set; }
}

public record PlanoResponse(Guid Id, string Nome, string? Descricao, decimal Valor, Periodicidade Periodicidade, DateTime? DataValidade);
