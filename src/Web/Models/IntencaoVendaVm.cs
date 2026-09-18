using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;

namespace Web.Models;

public class IntencaoVendaVm
{
    public Guid Id { get; set; }
    [Required] public string Nome { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Telefone { get; set; } = "";
    [Required] public string Cpf { get; set; } = "";
    [Required] public Guid PlanoId { get; set; }
    public int QuantidadeDependentes { get; set; }
    public string Endereco { get; set; } = "";
    public string Dependentes { get; set; } = "";
    public string PlanoNome { get; set; } = "";
    public decimal ValorPlano { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusIntencaoVenda Status { get; set; }
    public DateTime CriadoEm { get; set; }
    public bool NotificacaoSuspensa { get; set; }
}

public class IntencaoVendaEntradaVm : IntencaoVendaVm { }
