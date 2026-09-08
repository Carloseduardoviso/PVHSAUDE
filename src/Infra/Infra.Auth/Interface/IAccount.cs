using Infra.Auth;

namespace PVHSAUDE.Infra.Auth.Interface
{
    public interface IAccount
    {
        ProfileManager Current { get; }
        Access Access { get; }
    }
}
