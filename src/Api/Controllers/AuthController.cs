using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PVHSAUDE.Application.Interface;
using System.Runtime.Versioning;

namespace PVHSAUDE.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IAppJwtService _jwtService;
        private readonly IAppServiceUsuario _usuarioApp;

        public AuthController(ILogger<AuthController> logger, IAppServiceUsuario usuarioApp, IAppJwtService jwtService, IMemoryCache cache)
        {
            _logger = logger;
            _usuarioApp = usuarioApp;
            _jwtService = jwtService;
            _cache = cache;
        }
    }
}