using Microsoft.AspNetCore.Mvc;

using System.Net.Http;
using System.Threading.Tasks;
using Nexus.Logging.Configuration;
using Nexus.Logging.Contract;

namespace Nexus.Logging.Harness.Service2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SecondEndpoint : ControllerBase
    {
        private readonly ILogger<SecondEndpoint> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationScopeOptions _applicationScopeOptions;

        public SecondEndpoint(ILogger<SecondEndpoint> logger, IHttpClientFactory httpClientFactory, ApplicationScopeOptions applicationScopeOptions)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _applicationScopeOptions = applicationScopeOptions;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            _logger.Log(LogLevel.Info, $"Entering {nameof(SecondEndpoint)} in {_applicationScopeOptions.ApplicationName}");
            var httpClient = _httpClientFactory.CreateClient("ThirdService");
            var response = await httpClient.GetAsync("ThirdEndpoint");
            _logger.Log(LogLevel.Info, $"Return code for response from ThirdEndpoint: {response.StatusCode}");
            return $"Message returned form {nameof(SecondEndpoint)} and {await response.Content.ReadAsStringAsync()}";
        }
    }
}
