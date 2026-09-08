using Infra.Auth;
using PVHSAUDE.Infra.Auth.Interface;

namespace PVHSAUDE.Infra.Auth
{
    public class Account(ProfileManager profileManager, Access acess) : IAccount
    {
        public ProfileManager Current { get; } = profileManager;
        public Access Access { get; } = acess;
        public static string SSAUrl { get; set; } = string.Empty;
        public static string SSAUrlApi { get; set; } = string.Empty;
        public static string SSAUrlApiV2 { get; set; } = string.Empty;
    }
}