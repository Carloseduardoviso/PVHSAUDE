using PVHSAUDE.Application.ViewModels;

namespace Web.Models;

public record AcessosBeneficiarioVm(
    IReadOnlyCollection<AcessoBeneficiarioPendenteVm> Pendentes,
    IReadOnlyCollection<BeneficiarioVm> Beneficiarios);
