namespace PVHSAUDE.Domain.Entities;

public class EmpresaBeneficiadaEspecialidade
{
    public Guid EmpresaBeneficiadaId { get; private set; }
    public EmpresaBeneficiada EmpresaBeneficiada { get; private set; } = null!;
    public Guid EspecialidadeId { get; private set; }
    public Especialidade Especialidade { get; private set; } = null!;
    private EmpresaBeneficiadaEspecialidade() { }
    public EmpresaBeneficiadaEspecialidade(Guid empresaBeneficiadaId, Guid especialidadeId) { EmpresaBeneficiadaId = empresaBeneficiadaId; EspecialidadeId = especialidadeId; }
}
