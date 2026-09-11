using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Application.ViewModels;

public class EspecialidadeVm
{
    public Guid Id { get; set; }
    [Required] public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public class ProcedimentoVm
{
    public Guid Id { get; set; }
    [Required] public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public record CredenciadoEspecialidadeVm(Guid CredenciadoId, Guid EspecialidadeId);
public record CredenciadoProcedimentoVm(Guid CredenciadoId, Guid ProcedimentoId);
