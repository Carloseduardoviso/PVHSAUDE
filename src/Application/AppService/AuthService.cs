using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class AuthService(IEntityRepository<Usuario> repository, IEntityRepository<Beneficiario> beneficiarios, IUnitOfWork work, IMapper mapper, IPasswordHasher<Usuario> hasher, IAppJwtService jwt) : IAuthService
{
    private static readonly string DummyHash = new PasswordHasher<Usuario>().HashPassword(new Usuario(), Guid.NewGuid().ToString());
    public async Task<LoginResponse> LoginAsync(LoginVm vm, CancellationToken ct)
    {
        var email = vm.Email.Trim().ToUpperInvariant();
        var entity = await repository.ObterAsync(x => x.EmailNormalizado == email, ct);
        var result = hasher.VerifyHashedPassword(entity ?? new Usuario(), entity?.SenhaHash ?? DummyHash, vm.Senha);
        if (entity is null || !entity.Ativo || result == PasswordVerificationResult.Failed ||
            entity.Role == PVHSAUDE.Domain.Enuns.Role.Beneficiario && entity.BeneficiarioId is null)
            throw new ServiceException(ServiceError.Unauthorized);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            entity.SenhaHash = hasher.HashPassword(entity, vm.Senha);
            await work.SalvarAsync(ct);
        }
        var usuario = mapper.Map<UsuarioVm>(entity);
        return new LoginResponse(jwt.GenereteToken(usuario), usuario);
    }

    public async Task<LoginResponse> LoginCpfAsync(LoginCpfBeneficiarioVm vm, CancellationToken ct)
    {
        var cpf = new string(vm.Cpf.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11) throw new ServiceException(ServiceError.Unauthorized);
        var contas = await repository.ListarAsync(x => x.CpfSolicitado == cpf && x.Role == PVHSAUDE.Domain.Enuns.Role.Beneficiario && x.Ativo, ct);
        if (contas.Count == 0)
        {
            hasher.VerifyHashedPassword(new Usuario(), DummyHash, vm.Senha);
            throw new ServiceException(ServiceError.NotFound);
        }
        var entity = contas.Count == 1 ? contas[0] : null;
        var result = hasher.VerifyHashedPassword(entity ?? new Usuario(), entity?.SenhaHash ?? DummyHash, vm.Senha);
        if (entity?.BeneficiarioId is not Guid beneficiarioId || result == PasswordVerificationResult.Failed)
            throw new ServiceException(ServiceError.Unauthorized);
        var titular = await beneficiarios.ObterAsync(x => x.Id == beneficiarioId, ct, x => x.Dependentes);
        if (titular is null || titular.Cpf != cpf && !titular.Dependentes.Any(x => x.Cpf == cpf))
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
