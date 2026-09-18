namespace PVHSAUDE.Domain.Entities;
public class Contato { public Guid Id {get;set;}=Guid.NewGuid(); public string Nome{get;set;}=""; public string Sobrenome{get;set;}=""; public string Email{get;set;}=""; public string Telefone{get;set;}=""; public string Cpf{get;set;}=""; public string Motivo{get;set;}=""; public string Mensagem{get;set;}=""; public DateTime EnviadoEm{get;set;}=DateTime.UtcNow; public bool NotificacaoSuspensa { get; private set; } public void SuspenderNotificacao() => NotificacaoSuspensa = true; }

