using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PVHSAUDE.Domain.Enuns;

namespace Infra.Data.Config;

public static class GrauParentescoStorage
{
    public static string ParaTexto(GrauParentesco valor) =>
        typeof(GrauParentesco).GetField(valor.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name ?? "Outros";

    public static GrauParentesco ParaEnum(string texto)
    {
        foreach (var valor in Enum.GetValues<GrauParentesco>())
        {
            if (string.Equals(texto.Trim(), ParaTexto(valor), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(texto.Trim(), valor.ToString(), StringComparison.OrdinalIgnoreCase))
                return valor;
        }

        return texto.Trim().ToLowerInvariant() switch
        {
            "filha" or "filho" => GrauParentesco.Filho,
            "esposa" or "esposo" or "conjue" => GrauParentesco.Conjuge,
            "vó" => GrauParentesco.Avo,
            "vô" or "avô" => GrauParentesco.AvoMasculino,
            "tia" or "tio" => GrauParentesco.Tio,
            "irmã" or "irmão" => GrauParentesco.Irmao,
            "neta" or "neto" => GrauParentesco.Neto,
            "sobrinha" or "sobrinho" => GrauParentesco.Sobrinho,
            "prima" or "primo" => GrauParentesco.Primo,
            _ => GrauParentesco.Outros
        };
    }
}
