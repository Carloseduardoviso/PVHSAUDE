using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infra.Auth
{
    public class ProfileManager
    {
        private readonly ClaimsPrincipal _principal;

        public ProfileManager(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity()))
        {
        }

        public ProfileManager(ClaimsPrincipal principal)
        {
            _principal = principal ?? new ClaimsPrincipal(new ClaimsIdentity());

            if (!IsLogado) return;

            UsuarioId = Guid.Parse(FindFirst(ClaimTypes.NameIdentifier));
            Nome = FindFirst(ClaimTypes.GivenName);
        }

        public bool IsLogado => _principal?.Identity?.IsAuthenticated ?? false;

        public Guid UsuarioId { get; private set; }

        public string? Nome { get; private set; }

        public IEnumerable<Claim> Claims => _principal.Claims;

        private string FindFirst(string claimType)
        {
            return _principal?.FindFirst(claimType)?.Value ?? string.Empty;
        }

        public string GetApiToken()
        {
            return FindFirst(ClaimTypes.Authentication) ?? string.Empty;
        }
    }
}