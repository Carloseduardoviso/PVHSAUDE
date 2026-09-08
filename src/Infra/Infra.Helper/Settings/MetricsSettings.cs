using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Infra.Helper.Settings
{
    public class MetricsSettings
    {
        public const string Config = "Prometheus";

        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
