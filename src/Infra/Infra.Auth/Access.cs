using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PVHSAUDE.Infra.Helper.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace PVHSAUDE.Infra.Auth
{
    public class Access
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ApisSetting _apiSettings;

        public Access(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IOptions<ApisSetting> apisSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _apiSettings = apisSettings.Value;
        }

    }
}
