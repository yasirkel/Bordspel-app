using BordspelApp.WebApp.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace BordspelApp.WebApp.Services
{
    public class ApiBaseUrlProvider : IApiBaseUrlProvider
    {
        private readonly ApiOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ApiBaseUrlProvider(IOptions<ApiOptions> options, IWebHostEnvironment env, IConfiguration configuration)
        {
            _options = options.Value;
            _env = env;
            _configuration = configuration;
        }

        public bool IsProduction => _env?.IsProduction() ?? false;

        public string GetBaseUrl()
        {
            // Priority: explicit environment variable API_BASE_URL > Api:BaseUrl from config > sensible default
            var fromEnv = _configuration["API_BASE_URL"]; // Azure App Setting
            if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv!;

            if (!string.IsNullOrWhiteSpace(_options?.BaseUrl)) return _options.BaseUrl!;

            // fallback for development
            return "http://localhost:5247/";
        }
    }
}
