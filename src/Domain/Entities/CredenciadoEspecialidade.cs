namespace PVHSAUDE.Domain.Entities;

public class CredenciadoEspecialidade
{
    public Guid CredenciadoId { get; private set; }
    public Credenciado Credenciado { get; private set; } = null!;
    public Guid EspecialidadeId { get; private set; }
    public Especialidade Especialidade { get; private set; } = null!;
    private CredenciadoEspecialidade() { }
    public CredenciadoEspecialidade(Guid credenciadoId, Guid especialidadeId) { CredenciadoId = credenciadoId; EspecialidadeId = especialidadeId; }
}
