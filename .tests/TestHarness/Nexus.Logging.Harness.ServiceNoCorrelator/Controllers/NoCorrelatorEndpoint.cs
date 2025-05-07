using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Logging.Configuration;
using Nexus.Logging.Contract;

namespace Nexus.Logging.Harness.ServiceNoCorrelator.Controllers;

[ApiController]
[Route("[controller]")]
public class NoCorrelatorEndpoint : ControllerBase
{
    private readonly ApplicationScopeOptions _applicationScopeOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NoCorrelatorEndpoint> _logger;

    public NoCorrelatorEndpoint(ILogger<NoCorrelatorEndpoint> logger, IHttpClientFactory httpClientFactory,
        ApplicationScopeOptions applicationScopeOptions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _applicationScopeOptions = applicationScopeOptions;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        _logger.Log(LogLevel.Info,
            $"Entering {nameof(NoCorrelatorEndpoint)} in {_applicationScopeOptions.ApplicationName}");
        var httpClient = _httpClientFactory.CreateClient("ThirdService");
        var response = await httpClient.GetAsync("ThirdEndpoint");
        _logger.Log(LogLevel.Info, $"Return code for response from ThirdEndpoint: {response.StatusCode}");
        return $"Message returned form {nameof(NoCorrelatorEndpoint)} and {await response.Content.ReadAsStringAsync()}";
    }
}