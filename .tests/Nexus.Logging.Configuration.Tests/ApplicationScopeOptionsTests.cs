using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Nexus.Logging.Configuration.Tests
{
    public class ApplicationScopeOptionsTests
    {
        [Test]
        public void When_LoggerConfiguredWithValidOptions_Should_InitializeApplicationScopeOptions()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(new ConfigurationBuilder().AddJsonFile("testsettings.json").Build());

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<ApplicationScopeOptions>();

            Assert.That(options.ApplicationName, Is.EqualTo("ProgLeasing.System.Logging.Configuration.Tests123"));
            Assert.That(options.Environment, Is.EqualTo("UnitTests"));
        }

        [Test]
        public void When_LoggerConfiguredWithNoApplicationName_Should_DefaultToAssemblyName()
        {
            var services = new ServiceCollection();
            services.ConfigureLogging(new ConfigurationBuilder().AddJsonFile("testsettings.MissingAppName.json").Build());

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<ApplicationScopeOptions>();

            Assert.That(options.ApplicationName, Is.EqualTo("ProgLeasing.System.Logging.Configuration.Tests"));
            Assert.That(options.Environment, Is.EqualTo("UnitTests"));
        }

        [Test]
        public void When_LoggerConfiguredWithMissingEnv_Should_DefaultToAssemblyName()
        {
            var services = new ServiceCollection();

            Assert.Throws<LoggerConfigurationException>(
                () => services.ConfigureLogging(new ConfigurationBuilder().AddJsonFile("testsettings.MissingEnv.json").Build()));
        }
    }
}