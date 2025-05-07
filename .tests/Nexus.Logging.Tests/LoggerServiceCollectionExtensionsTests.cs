using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Logging.Configuration;
using NUnit.Framework;

namespace Nexus.Logging.Tests;

public class LoggerServiceCollectionExtensionsTests
{
    [Test]
    public void When_ConfiguringLoggerWithoutEnvironment_Should_ThrowLoggerConfigurationException()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("testsettingsWoEnv.json", false, false)
            .Build();

        var services = new ServiceCollection();

        Assert.That(
            () => services.AddNexusLogger(configuration, builder =>
                builder.RegisterLoggerProvider(new ConfigureInMemoryProvider())),
            Throws.TypeOf<LoggerConfigurationException>());
    }

    [Test]
    public void When_ConfiguringLoggerWithoutAppName_Should_DefaultToAssemblyName()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("testsettingsWoAppName.json", false, false)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(configuration, builder =>
            builder.RegisterLoggerProvider(new ConfigureInMemoryProvider()));

        var sp = services.BuildServiceProvider();
        var appScope = sp.GetRequiredService<ApplicationScope>();

        Assert.That(appScope, Is.Not.Null);
        Assert.That(
            appScope.Options.ApplicationName,
            Is.EqualTo("Nexus.Logging"));
    }

    [Test]
    public void When_ConfiguringWithAppNameAndEnvironment_Should_PopulateOptions()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("testsettings.json", false, false)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(configuration, builder =>
            builder.RegisterLoggerProvider(new ConfigureInMemoryProvider()));

        var sp = services.BuildServiceProvider();
        var appScope = sp.GetRequiredService<ApplicationScope>();

        Assert.That(appScope, Is.Not.Null);
        Assert.That(
            appScope.Options.ApplicationName,
            Is.EqualTo("Logging.Tests"));
        Assert.That(
            appScope.Options.Environment,
            Is.EqualTo("UnitTests"));
    }
}