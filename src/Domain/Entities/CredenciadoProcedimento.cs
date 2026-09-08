namespace PVHSAUDE.Domain.Entities;

public class CredenciadoProcedimento
{
    public Guid CredenciadoId { get; private set; }
    public Credenciado Credenciado { get; private set; } = null!;
    public Guid ProcedimentoId { get; private set; }
    public Procedimento Procedimento { get; private set; } = null!;
    private CredenciadoProcedimento() { }
    public CredenciadoProcedimento(Guid credenciadoId, Guid procedimentoId) { CredenciadoId = credenciadoId; ProcedimentoId = procedimentoId; }
}
