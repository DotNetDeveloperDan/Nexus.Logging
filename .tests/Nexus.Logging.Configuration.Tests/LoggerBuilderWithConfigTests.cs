using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework; // Ensure this is included for NUnit's Assert methods

namespace Nexus.Logging.Configuration.Tests
{
    public class LoggerBuilderWithConfigTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Init()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                .Build();
        }

        [Test]
        public void When_ConfigureLoggingThenBuild_Should_SetupTestLoggerProvider()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(_configuration)
                .RegisterLoggerProvider(new MELConsoleLoggerProvider())
                .Build();

            var sp = services.BuildServiceProvider();

            // Validate that the filters are present in the configured options
            var filters = sp.GetRequiredService<IOptionsSnapshot<LoggerFilterOptions>>().Value;
            Assert.That(filters, Is.Not.Null);
            Assert.That(filters.Rules.Count, Is.EqualTo(2)); // Updated to use NUnit's Assert.That

            // ConsoleLoggerOptions will only exist if the console logger was added
            var console = sp.GetRequiredService<IOptionsSnapshot<ConsoleLoggerOptions>>().Value;
            Assert.That(console, Is.Not.Null);
            Assert.That(console.IncludeScopes, Is.True); // Updated to use NUnit's Assert.That

            // Verify the configured ILogger can be created
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(LoggerBuilderWithConfigTests));
            Assert.That(logger, Is.Not.Null);
            // Verify that the MinimumLevel did not end up with the default value
            Assert.That(logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning), Is.True);
            logger.LogCritical("Testing logging critical message log level :) =========> See me in the window!!");
        }
    }
}
