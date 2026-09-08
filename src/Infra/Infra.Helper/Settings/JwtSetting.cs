using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Infra.Helper.Settings
{
    public class JwtSetting
    {
        public const string Config = "Jwt";

        [Required]
        public string SecretKey { get; set; } = string.Empty;
        [Required]
        public string Issuer { get; set; } = string.Empty;
        [Required]
        public string Audience { get; set; } = string.Empty;
    }
}