using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Logging.Configuration;
using Nexus.Logging.Contract;

namespace Nexus.Logging.Harness.Service1.Controllers;

[ApiController]
[Route("[controller]")]
public class FirstEndpoint : ControllerBase
{
    private readonly ApplicationScopeOptions _applicationScopeOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FirstEndpoint> _logger;

    public FirstEndpoint(ILogger<FirstEndpoint> logger, IHttpClientFactory httpClientFactory,
        ApplicationScopeOptions applicationScopeOptions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _applicationScopeOptions = applicationScopeOptions;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        _logger.Log(LogLevel.Info, $"Entering {nameof(FirstEndpoint)} in {_applicationScopeOptions.ApplicationName}");
        var httpClient = _httpClientFactory.CreateClient("SecondService");
        var response = await httpClient.GetAsync("SecondEndpoint");
        _logger.Log(LogLevel.Info, $"Return code for response from SecondEndpoint: {response.StatusCode}");
        var httpClient2 = _httpClientFactory.CreateClient("ServiceNoCorrelator");
        var response2 = await httpClient2.GetAsync("NoCorrelatorEndpoint");
        _logger.Log(LogLevel.Info, $"Return code for response from NoCorrelatorEndpoint: {response.StatusCode}");
        return
            $"Message returned form {nameof(FirstEndpoint)} and {await response.Content.ReadAsStringAsync()} and {await response2.Content.ReadAsStringAsync()}";
    }
}