using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;
public record EmpresaPlanoVm(Guid CredenciadoId, Guid PlanoId, string Plano, decimal Valor, Periodicidade Periodicidade);
