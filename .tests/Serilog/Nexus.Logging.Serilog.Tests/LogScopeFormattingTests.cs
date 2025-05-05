using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Logging.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Serilog.Tests
{
    public class LogScopeFormattingTests
    {
        private ILogger<LogScopeFormattingTests> _logger;

        [SetUp]
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("test.inMemoryTarget.settings.json", optional: false, reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();
            services.AddNexusLogger(config, b => b.RegisterSerilog());
            var sp = services.BuildServiceProvider();
            _logger = sp.GetRequiredService<ILogger<LogScopeFormattingTests>>();
        }

        [Test]
        public void When_LoggingWithStructuredScope_Should_DestructureScopeToLogDetails()
        {
            var meta = new Dictionary<string, object>
            {
                ["@AnonymousType"] = new { Prop1 = "StringValue1", Prop2 = 123456, Prop3 = true }
            };
            _logger.Log(LogLevel.Info, "Testing message", metadata: meta);

            var message = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message,
                Does.Contain("\"AnonymousType\":{\"Prop1\":\"StringValue1\",\"Prop2\":123456,\"Prop3\":true}")
            );
        }

        [Test]
        public void When_LoggingWithObjectMetadata_Should_DestructureObjectPropertiesToLogDetails()
        {
            var meta = new Dictionary<string, object>
            {
                ["@FooScope"] = new FooScopeNoToString()
            };
            _logger.Log(LogLevel.Info, "Testing message", metadata: meta);

            var message = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message,
                Does.Contain("\"FooScope\":{\"Foo1\":\"FooVal1\",\"Foo2\":false}")
            );
        }

        [Test]
        public void When_LoggingWithStructuredScopeNoDestructure_Should_StringifyToLogDetails()
        {
            var meta = new Dictionary<string, object>
            {
                ["AnonymousType"] = new { Prop1 = "StringValue1", Prop2 = 123456, Prop3 = true }
            };
            _logger.Log(LogLevel.Info, "Testing message", metadata: meta);

            var message = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message,
                Does.Contain("\"AnonymousType\":\"{ Prop1 = StringValue1, Prop2 = 123456, Prop3 = True }\"")
            );
        }

        [Test]
        public void When_LoggingWithObjectMetadataNoDestructureNoToString_Should_ObjectToStringInLogDetails()
        {
            var meta = new Dictionary<string, object>
            {
                ["FooScope"] = new FooScopeNoToString()
            };
            _logger.Log(LogLevel.Info, "Testing message", metadata: meta);

            var message = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message,
                Does.Contain("\"FooScope\":\"ProgLeasing.System.Logging.Serilog.Tests.FooScopeNoToString\"")
            );
        }

        [Test]
        public void When_LoggingWithObjectMetadataNoDestructureToString_Should_UseOverrideToStringInLogDetails()
        {
            var meta = new Dictionary<string, object>
            {
                ["FooScope"] = new FooScopeWithToString()
            };
            _logger.Log(LogLevel.Info, "Testing message", metadata: meta);

            var message = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message,
                Does.Contain("\"FooScope\":\"Foo1+FooVal1;Foo2+False\"")
            );
        }
    }

    public class FooScopeNoToString
    {
        public string Foo1 { get; set; } = "FooVal1";
        public bool Foo2 { get; set; } = false;
    }

    public class FooScopeWithToString
    {
        public string Foo1 { get; set; } = "FooVal1";
        public bool Foo2 { get; set; } = false;

        public override string ToString() => $"{nameof(Foo1)}+{Foo1};{nameof(Foo2)}+{Foo2}";
    }
}
