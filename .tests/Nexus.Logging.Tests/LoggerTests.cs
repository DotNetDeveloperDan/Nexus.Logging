using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Logging.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Tests;

public class LoggerTests
{
    private IConfiguration _configuration;
    private ILogger<LoggerTests> _logger;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Init()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("testsettings.json", false, false)
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(_configuration,
            builder => { builder.RegisterLoggerProvider(new ConfigureInMemoryProvider()); });

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<LoggerTests>>();
    }

    [Test]
    public void When_LoggerIsConfigured_Should_InjectCorrectLogger()
    {
        Assert.That(_logger, Is.Not.Null);
        Assert.That(_logger.GetType(), Is.EqualTo(typeof(Logger<LoggerTests>)));
    }

    [Test]
    public void When_LoggerIsUsed_Should_Log()
    {
        const string message = "DID I WORK?";
        _logger.Log(LogLevel.Error, message);
        var log = InMemoryLogger.PopLog();

        Assert.That(log.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Error));
        Assert.That(log.Message, Does.Contain(message));
    }

    [Test]
    public void When_LogHasMetadata_Should_AddToLogDetailsScope()
    {
        const string message = "DID I WORK?";
        const string scopeKey = "ScopeAddedInline";
        const string scopeValue = "ValueOfScopeAddedInline";

        _logger.Log(
            LogLevel.Error,
            message,
            metadata: new Dictionary<string, object> { [scopeKey] = scopeValue }
        );
        var log = InMemoryLogger.PopLog();

        Assert.That(log.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Error));
        Assert.That(log.Message, Does.Contain(message));
        Assert.That(log.Message, Does.Contain(scopeKey));
        Assert.That(log.Message, Does.Contain(scopeValue));
    }

    [Test]
    public void When_LogHasMessageTemplateAndMetadata_Should_FormatLogMessageAndAddScopes()
    {
        const string message = "DID I WORK TO {Swap}?";
        const string expectedMessage = "DID I WORK TO SWAPPED?";
        const string scopeKey = "Swap";
        const string scopeValue = "SWAPPED";

        _logger.Log(
            LogLevel.Error,
            message,
            metadata: new Dictionary<string, object> { [scopeKey] = scopeValue }
        );
        var log = InMemoryLogger.PopLog();

        Assert.That(log.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Error));
        Assert.That(log.Message, Does.Contain(expectedMessage));
        Assert.That(log.Message, Does.Contain(scopeKey));
        Assert.That(log.Message, Does.Contain(scopeValue));
    }

    [Test]
    public void When_LogHasMessageTemplateWithoutMetadata_Should_LogWithUnformattedLogMessage()
    {
        const string message = "DID I WORK TO {Swap}?";

        _logger.Log(LogLevel.Error, message);
        var log = InMemoryLogger.PopLog();

        Assert.That(log.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Error));
        Assert.That(log.Message, Does.Contain(message));
    }

    [Test]
    public void When_LogMessageIsEmpty_Should_LogDefaultNullMessage()
    {
        _logger.Log(LogLevel.Debug, "");
        var log = InMemoryLogger.PopLog();

        Assert.That(log.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Debug));
        Assert.That(log.Message, Does.Contain("[null]"));
    }

    [Test]
    public void When_ScopeWrapsLogs_Should_MaintainScopeOnEachLog()
    {
        const string infoMessage = "DID I INFO?";
        const string warnMessage = "DID I WARN?";
        const string scopeKey = "Persisted";
        const string scopeValue = "AmIHereOnBothOfTheMessages";

        using (_logger.BeginScope(new Dictionary<string, object> { [scopeKey] = scopeValue }))
        {
            _logger.Log(LogLevel.Info, infoMessage);
            _logger.Log(LogLevel.Warn, warnMessage);
        }

        var firstLog = InMemoryLogger.PopLog();
        Assert.That(firstLog.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Information));
        Assert.That(firstLog.Message, Does.Contain(infoMessage));
        Assert.That(firstLog.Message, Does.Contain(scopeKey));
        Assert.That(firstLog.Message, Does.Contain(scopeValue));

        var secondLog = InMemoryLogger.PopLog();
        Assert.That(secondLog.LogLevel, Is.EqualTo(Microsoft.Extensions.Logging.LogLevel.Warning));
        Assert.That(secondLog.Message, Does.Contain(warnMessage));
        Assert.That(secondLog.Message, Does.Contain(scopeKey));
        Assert.That(secondLog.Message, Does.Contain(scopeValue));
    }

    [TearDown]
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}