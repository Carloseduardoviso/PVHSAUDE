namespace PVHSAUDE.Domain.Entities;

public class EmpresaBeneficiadaProcedimento
{
    public Guid EmpresaBeneficiadaId { get; private set; }
    public EmpresaBeneficiada EmpresaBeneficiada { get; private set; } = null!;
    public Guid ProcedimentoId { get; private set; }
    public Procedimento Procedimento { get; private set; } = null!;
    private EmpresaBeneficiadaProcedimento() { }
    public EmpresaBeneficiadaProcedimento(Guid empresaBeneficiadaId, Guid procedimentoId) { EmpresaBeneficiadaId = empresaBeneficiadaId; ProcedimentoId = procedimentoId; }
}
