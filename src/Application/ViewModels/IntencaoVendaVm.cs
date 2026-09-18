using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Application.ViewModels;

public class IntencaoVendaEntradaVm
{
    [Required, MaxLength(150)] public string Nome { get; set; } = "";
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = "";
    [Required, MaxLength(30)] public string Telefone { get; set; } = "";
    [Required, MaxLength(20)] public string Cpf { get; set; } = "";
    [Required] public Guid PlanoId { get; set; }
    [Range(0, 5)] public int QuantidadeDependentes { get; set; }
    [MaxLength(500)] public string Endereco { get; set; } = "";
    [MaxLength(4000)] public string Dependentes { get; set; } = "";
}

public class IntencaoVendaVm : IntencaoVendaEntradaVm
{
    public Guid Id { get; set; }
    public string PlanoNome { get; set; } = "";
    public decimal ValorPlano { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusIntencaoVenda Status { get; set; }
    public DateTime CriadoEm { get; set; }
    public bool NotificacaoSuspensa { get; set; }
}
