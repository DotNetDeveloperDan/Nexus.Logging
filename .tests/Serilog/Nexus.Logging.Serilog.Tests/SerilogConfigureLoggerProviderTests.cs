using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nexus.Logging.Configuration;
using NUnit.Framework;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using LogLevel = Nexus.Logging.Contract.LogLevel;
using SL = Serilog;

namespace Nexus.Logging.Serilog.Tests;

public class SerilogConfigureLoggerProviderTests
{
    [TestCase("$NexusJsonLogFormatter", typeof(NexusJsonLogFormatter))]
    [TestCase("[{Timestamp}] {Message} {Exception}{NewLine}", typeof(MessageTemplateTextFormatter))]
    [TestCase("", typeof(NexusJsonLogFormatter))]
    [TestCase(null, typeof(NexusJsonLogFormatter))]
    public void When_OutputTemplateProvided_Should_ReturnCorrectFormatter(string outputTemplate, Type expectedFormatter)
    {
        var provider = new SerilogConfigureLoggerProvider();
        var formatter = provider.GetOutputFormatter(outputTemplate);

        Assert.That(formatter.GetType(), Is.EqualTo(expectedFormatter));
    }

    [Test]
    public void When_SerilogConfigured_Should_RegisterSerilogLoggerProvider()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("testSettings.json", false, false)
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(config, b => b.RegisterSerilog());
        var sp = services.BuildServiceProvider();
        var loggerProvider = sp.GetRequiredService<ILoggerProvider>();

        Assert.That(loggerProvider, Is.Not.Null);
        Assert.That(loggerProvider, Is.InstanceOf<SerilogLoggerProvider>());
    }

    [Test]
    public void When_SerilogConfiguredWithAddNexusLogger_Should_LogThroughSerilogSinkFromContractILogger()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("test.serviceTarget.settings.json", false, false)
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(config, b => b.RegisterSerilog());
        var sp = services.BuildServiceProvider();

        var logger = sp.GetRequiredService<Contract.ILogger<SerilogConfigureLoggerProviderTests>>();
        Assert.That(logger, Is.Not.Null);

        const string logMessage = "Test log message";
        logger.Log(LogLevel.Info, logMessage);

        var log = InMemorySink.InMemorySink.Pop();
        Assert.That(log, Is.Not.Null);
        Assert.That(log, Does.Contain(logMessage));
    }

    [Test]
    public void When_SerilogConfiguredForRollingFileWithMissingPathFormat_Should_ThrowLoggerConfigurationException()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("test.rollingFileTargetMissing.settings.json", false, false)
            .Build();

        var services = new ServiceCollection();

        Assert.That(
            () => services.AddNexusLogger(config, b => b.RegisterSerilog()),
            Throws.TypeOf<LoggerConfigurationException>()
                .With.Message.Contain("PathFormat")
        );
    }

    [Test]
    public void When_SerilogConfiguredForRollingFileWithInaccessiblePathFormat_Should_NotThrow()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("test.rollingFileTargetNotAccessible.settings.json", false, false)
            .Build();

        var services = new ServiceCollection();
        Assert.That(
            () => services.AddNexusLogger(config, b => b.RegisterSerilog()),
            Throws.Nothing
        );

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<Contract.ILogger<SerilogConfigureLoggerProviderTests>>();
        Assert.That(logger, Is.Not.Null);
    }

    [Test]
    public void When_SerilogConfiguredForRollingFile_Should_ConfigureSinkWithTokenReplacedLogFilePathFormat()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("test.rollingFile.settings.json", false, false)
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(config, b => b.RegisterSerilog());
        var sp = services.BuildServiceProvider();
        var logProvider = sp.GetRequiredService<ILoggerProvider>();

        Assert.That(logProvider, Is.InstanceOf<SerilogLoggerProvider>());

        // reflect into Serilog
        var serilogLogger = (SL.ILogger)logProvider.GetType()
            .GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(logProvider);

        var loggerSink = serilogLogger.GetType()
            .GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(serilogLogger);

        var aggregateSink = loggerSink.GetType()
            .GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(loggerSink);

        var sinks = (ILogEventSink[])aggregateSink.GetType()
            .GetField("_sinks", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(aggregateSink);

        var configured = false;
        foreach (var sink in sinks)
            if (sink.GetType().FullName.Contains("RestrictedSink"))
            {
                var internalSink = sink.GetType()
                    .GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(sink);
                if (internalSink.GetType().FullName.Contains("RollingFileSink"))
                {
                    var roller = internalSink.GetType()
                        .GetField("_roller", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(internalSink);

                    var dir = (string)roller.GetType().GetProperty("LogFileDirectory").GetValue(roller);
                    var pattern = (string)roller.GetType().GetProperty("DirectorySearchPattern").GetValue(roller);
                    var logFile = Path.Combine(dir, pattern);

                    Assert.That(logFile, Is.Not.Null.And.Not.Empty);
                    Assert.That(logFile, Does.Not.Contain("{Environment}"));
                    Assert.That(logFile, Does.Not.Contain("{ApplicationName}"));

                    configured = true;
                }
            }

        Assert.That(configured, Is.True);
    }
}