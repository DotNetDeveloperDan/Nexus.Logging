using Microsoft.AspNetCore.Mvc;
using Nexus.Logging.Configuration;
using Nexus.Logging.Contract;

namespace Nexus.Logging.Harness.Service3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ThirdEndpoint : ControllerBase
    {
        private readonly ILogger<ThirdEndpoint> _logger;
        private readonly ApplicationScopeOptions _applicationScopeOptions;

        public ThirdEndpoint(ILogger<ThirdEndpoint> logger, ApplicationScopeOptions applicationScopeOptions)
        {
            _logger = logger;
            _applicationScopeOptions = applicationScopeOptions;
        }

        [HttpGet]
        public string Get()
        {
            _logger.Log(LogLevel.Info, $"Entered {nameof(ThirdEndpoint)} in {_applicationScopeOptions.ApplicationName}");
            return $"Message returned form {nameof(ThirdEndpoint)}";
        }
    }
}
