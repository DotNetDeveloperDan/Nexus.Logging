using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Logging.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Serilog.Tests;

public class LogOutputFormattingTests
{
    private ILogger<LogContentVerificationTests> _logger;

    [SetUp]
    public void Init()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("test.newLineLogs.settings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddNexusLogger(config, builder => { builder.RegisterSerilog(); });
        var sp = services.BuildServiceProvider();
        _logger = sp.GetRequiredService<ILogger<LogContentVerificationTests>>();
    }

    [Test]
    public void When_LoggingMultipleTimes_Should_LogMessagesOnSeparateLines()
    {
        var message1 = "First message logged to line 1";
        var message2 = "Second message logged to line 2";
        _logger.Log(LogLevel.Info, message1);
        _logger.Log(LogLevel.Info, message2);

        using var reader = new StringReader(FileSimulatorSink.FileSimulatorSink.GetLogs());
        var line1 = reader.ReadLine();
        var line2 = reader.ReadLine();

        Assert.That(line1.Contains(message1), Is.True);
        Assert.That(line2.Contains(message2), Is.True);
    }
}