using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Nexus.Logging.Configuration.Tests
{
    public class LoggerBuilderWithoutConfigTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Init()
        {
            _configuration = new ConfigurationBuilder()
                .Build();
        }

        [Test]
        public void When_ConfigureLoggingWithoutRegisteringProviderThenBuild_Should_ThrowLoggerConfigurationException()
        {
            var services = new ServiceCollection();
            var ex = Assert.Throws<LoggerConfigurationException>(() => services.ConfigureLogging(_configuration).Build());
        }

    }
}
