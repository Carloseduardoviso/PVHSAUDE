using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class AuthService(IEntityRepository<Usuario> repository, IUnitOfWork work, IMapper mapper, IPasswordHasher<Usuario> hasher, IAppJwtService jwt) : IAuthService
{
    private static readonly string DummyHash = new PasswordHasher<Usuario>().HashPassword(new Usuario(), Guid.NewGuid().ToString());
    public async Task<LoginResponse> LoginAsync(LoginVm vm, CancellationToken ct)
    {
        var email = vm.Email.Trim().ToUpperInvariant();
        var entity = await repository.ObterAsync(x => x.EmailNormalizado == email, ct);
        var result = hasher.VerifyHashedPassword(entity ?? new Usuario(), entity?.SenhaHash ?? DummyHash, vm.Senha);
        if (entity is null || !entity.Ativo || result == PasswordVerificationResult.Failed)
            throw new ServiceException(ServiceError.Unauthorized);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            entity.SenhaHash = hasher.HashPassword(entity, vm.Senha);
            await work.SalvarAsync(ct);
        }
        var usuario = mapper.Map<UsuarioVm>(entity);
        return new LoginResponse(jwt.GenereteToken(usuario), usuario);
    }
}
