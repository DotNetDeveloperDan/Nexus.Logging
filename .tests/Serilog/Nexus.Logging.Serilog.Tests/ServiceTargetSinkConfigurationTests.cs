using System.Collections.Generic;
using Nexus.Logging.Configuration;
using NUnit.Framework;
using Serilog;

namespace Nexus.Logging.Serilog.Tests
{
    public class ServiceTargetSinkConfigurationTests
    {
        [Test]
        public void When_ConfiguringServiceLoggerTargetWithValidParameters_Should_RegisterServiceLoggerTarget()
        {
            var target = new LoggerTargetOptions
            {
                Provider = "Serilog",
                Type = LoggerTarget.Service,
                Args = new Dictionary<string, string>
                {
                    { "Using", "ProgLeasing.System.Logging.Serilog.InMemorySink" },
                    { "Name",  "InMemory" }
                }
            };
            var loggerConfig = new LoggerConfiguration();

            Assert.That(
                () => ServiceTargetSinkConfiguration.Register(
                          target,
                          new NexusJsonLogFormatter(),
                          loggerConfig.WriteTo),
                Throws.Nothing,
                "Unexpected exception when registering a valid service sink");
        }

        [Test]
        public void When_ConfiguringServiceLoggerTargetWithMissingNameArg_Should_ThrowException()
        {
            var target = new LoggerTargetOptions
            {
                Provider = "Serilog",
                Type = LoggerTarget.Service,
                Args = new Dictionary<string, string>
                {
                    { "Using", "ProgLeasing.System.Logging.Serilog.InMemorySink" }
                }
            };
            var loggerConfig = new LoggerConfiguration();

            Assert.That(
                () => ServiceTargetSinkConfiguration.Register(
                          target,
                          new NexusJsonLogFormatter(),
                          loggerConfig.WriteTo),
                Throws.TypeOf<LoggerConfigurationException>()
                      .With.Message.Contain("'Name'"));
        }

        [Test]
        public void When_ConfiguringServiceLoggerTargetWithMissingUsingArg_Should_ThrowException()
        {
            var target = new LoggerTargetOptions
            {
                Provider = "Serilog",
                Type = LoggerTarget.Service,
                Args = new Dictionary<string, string>
                {
                    { "Name", "InMemory" }
                }
            };
            var loggerConfig = new LoggerConfiguration();

            Assert.That(
                () => ServiceTargetSinkConfiguration.Register(
                          target,
                          new NexusJsonLogFormatter(),
                          loggerConfig.WriteTo),
                Throws.TypeOf<LoggerConfigurationException>()
                      .With.Message.Contain("'Using'"));
        }
    }
}
