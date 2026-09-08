using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Infra.Helper.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PVHSAUDE.Application.AppService
{
    public class AppJwtService : IAppJwtService
    {
        private readonly JwtSetting _jwtSettings;
        public AppJwtService(IOptions<JwtSetting> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        public string GenereteToken(UsuarioVm usuarioVm)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, usuarioVm.UsuarioId.ToString()),
                    new(ClaimTypes.Name, usuarioVm.Email!),
                    new(ClaimTypes.GivenName, usuarioVm.NomeCompleto!),
                    new(ClaimTypes.Role, usuarioVm.Role.ToString()),
                ]),
                Expires = DateTime.UtcNow.AddHours(6),
                IssuedAt = DateTime.UtcNow,
                Issuer = _jwtSettings.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);


            return tokenHandler.WriteToken(token);
        }

        public string GenereteTokenForgotPassword(UsuarioVm usuarioVm)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, usuarioVm.UsuarioId.ToString()),
                    new("Purpose", "ForgotPassword")
                ]),
                Expires = DateTime.UtcNow.AddMinutes(30),
                IssuedAt = DateTime.UtcNow,
                Issuer = _jwtSettings.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);


            return tokenHandler.WriteToken(token);
        }

        public string? GetIdentifierToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            ClaimsPrincipal principal = CreateClainsPrincipal(token, tokenHandler, key);

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        }

        public bool ValidateTokenForgotPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            try
            {
                ClaimsPrincipal principal = CreateClainsPrincipal(token, tokenHandler, key);

                var purposeClaim = principal.FindFirst("Purpose");
                return purposeClaim?.Value == "ForgotPassword";
            }
            catch
            {
                return false;
            }
        }

        private ClaimsPrincipal CreateClainsPrincipal(string token, JwtSecurityTokenHandler tokenHandler, byte[] key)
        {
            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateIssuerSigningKey = true
            }, out _);
        }
    }
}