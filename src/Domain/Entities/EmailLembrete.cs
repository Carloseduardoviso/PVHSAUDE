namespace PVHSAUDE.Domain.Entities;

public class EmailLembrete
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PessoaId { get; private set; }
    public DateTime DataValidade { get; private set; }
    public int DiasAntes { get; private set; }
    public string Destinatario { get; private set; } = string.Empty;
    public bool Enviado { get; private set; }
    public DateTime? EnviadoEm { get; private set; }

    private EmailLembrete() { }

    public EmailLembrete(Guid pessoaId, DateTime dataValidade, int diasAntes, string destinatario)
    {
        PessoaId = pessoaId;
        DataValidade = dataValidade.Date;
        DiasAntes = diasAntes;
        Destinatario = destinatario.Trim();
    }

    public void MarcarEnviado(DateTime enviadoEm)
    {
        Enviado = true;
        EnviadoEm = enviadoEm;
    }
}
