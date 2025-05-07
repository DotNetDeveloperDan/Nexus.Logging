
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexus.Logging.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Configuration.Tests
{
    public class LoggerOptionsTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Init()
        {
            // Point at the folder where the test DLL lives so testsettings.json is found
            _configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("testsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }

        [Test]
        public void When_LoggerOptionsConfiguredFromIConfiguration_Should_MatchLoggerOptions()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(_configuration);

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<LoggerOptions>>().Value;

            Assert.That(options, Is.Not.Null);
            Assert.That(options.Filters.Count, Is.EqualTo(2));
            Assert.That(options.Targets.Count, Is.EqualTo(3));
            Assert.That(options.MinimumLevel.HasValue, Is.True);
            Assert.That(options.MinimumLevel.GetValueOrDefault(), Is.EqualTo(LogLevel.Warn));
        }

        [Test]
        public void When_LoggerOptionsHasFiltersConfiguredFromIConfiguration_Should_HaveFiltersConfigured()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(_configuration);

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<LoggerOptions>>().Value;

            Assert.That(options, Is.Not.Null);
            Assert.That(options.Filters.Count, Is.EqualTo(2));

            Assert.That(options.Filters.ContainsKey("Microsoft"), Is.True);
            Assert.That(options.Filters["Microsoft"], Is.EqualTo(LogLevel.Error));

            Assert.That(options.Filters.ContainsKey("System"), Is.True);
            Assert.That(options.Filters["System"], Is.EqualTo(LogLevel.Info));
        }

        [Test]
        public void When_LoggerOptionsHasTargetsConfiguredFromIConfiguration_Should_HaveLoggerTargetOptionsConfigured()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(_configuration);

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptions<LoggerOptions>>().Value;

            Assert.That(options, Is.Not.Null);
            Assert.That(options.Targets.Count, Is.EqualTo(3));

            // Console target
            var consoleTarget = options.Targets.FirstOrDefault(t => t.Type == LoggerTarget.Console);
            Assert.That(consoleTarget, Is.Not.Null);
            Assert.That(consoleTarget.OutputTemplate, Is.EqualTo("TestingOverrideForTemplate"));
            Assert.That(consoleTarget.Args.Count, Is.EqualTo(1));
            Assert.That(consoleTarget.Args.ContainsKey("ImAKeyExample"), Is.True);
            Assert.That(consoleTarget.Args["ImAKeyExample"], Is.EqualTo("ImAValueForAKey"));

            // RollingFile target
            var rollingfileTarget = options.Targets.FirstOrDefault(t => t.Type == LoggerTarget.RollingFile);
            Assert.That(rollingfileTarget, Is.Not.Null);
            Assert.That(rollingfileTarget.OutputTemplate, Is.Null);
            Assert.That(rollingfileTarget.Args.Count, Is.EqualTo(3));
            Assert.That(rollingfileTarget.Args.ContainsKey("FileCount"), Is.True);
            Assert.That(rollingfileTarget.Args["FileCount"], Is.EqualTo("15"));
            Assert.That(rollingfileTarget.Args.ContainsKey("PathFormat"), Is.True);
            Assert.That(rollingfileTarget.Args["PathFormat"], Is.EqualTo(@"C:\Test\Path\Magic.log"));
            // (you can add an Assert for that third arg here if needed)

            // Debug target
            var debugTarget = options.Targets.FirstOrDefault(t => t.Type == LoggerTarget.Debug);
            Assert.That(debugTarget, Is.Not.Null);
            Assert.That(debugTarget.OutputTemplate, Is.Null);
            Assert.That(debugTarget.Args.Count, Is.EqualTo(0));
        }
    }
}
