using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginVm vm, CancellationToken ct);
    Task<LoginResponse> LoginCpfAsync(LoginCpfBeneficiarioVm vm, CancellationToken ct);
}
