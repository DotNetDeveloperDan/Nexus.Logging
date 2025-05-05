using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Logging.Contract;
using NUnit.Framework;

namespace Nexus.Logging.Serilog.Tests
{
    public class LogContentVerificationTests
    {
        private ILogger<LogContentVerificationTests> _logger;

        [SetUp]
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("test.serviceTarget.settings.json", optional: false, reloadOnChange: false)
                .Build();

            var services = new ServiceCollection();
            services.AddNexusLogger(config, b => b.RegisterSerilog());
            var sp = services.BuildServiceProvider();
            _logger = sp.GetRequiredService<ILogger<LogContentVerificationTests>>();
        }

        [Test]
        public void When_LoggingScopeInline_Should_IncludeScopeInLogDetails()
        {
            _logger.Log(
                LogLevel.Info,
                "This is a log message",
                metadata: new Dictionary<string, object> { ["LeaseId"] = "LTEST654987" }
            );

            var message = InMemorySink.InMemorySink.Pop();

            Assert.That(message, Does.Contain("This is a log message"));
            Assert.That(message, Does.Contain("\"LeaseId\":\"LTEST654987\""));
        }

        [Test]
        public void When_LoggingWithParentScope_Should_IncludeParentScopeInChildLogs()
        {
            _logger.Log(LogLevel.Info, "Outside of scope");
            using (_logger.BeginScope(new Dictionary<string, object> { ["LeaseId"] = "LTEST654987" }))
            {
                _logger.Log(LogLevel.Info, "Log without declaring scope should include scope");
            }

            _logger.Log(LogLevel.Info, "Outside of scope again");

            var message1 = InMemorySink.InMemorySink.Pop();
            var message2 = InMemorySink.InMemorySink.Pop();
            var message3 = InMemorySink.InMemorySink.Pop();

            Assert.That(message1, Does.Not.Contain("\"LeaseId\":\"LTEST654987\""));
            Assert.That(message2, Does.Contain("\"LeaseId\":\"LTEST654987\""));
            Assert.That(message3, Does.Not.Contain("\"LeaseId\":\"LTEST654987\""));
        }

        [Test]
        public void When_LoggingWithMessageTemplate_Should_FormatMessageTemplate()
        {
            _logger.Log(
                LogLevel.Info,
                "This is a log message that includes LeaseId: {LeaseId}",
                metadata: new Dictionary<string, object> { ["LeaseId"] = "LTEST654987" }
            );

            var message1 = InMemorySink.InMemorySink.Pop();

            Assert.That(
                message1,
                Does.Contain("This is a log message that includes LeaseId: LTEST654987")
            );
            Assert.That(message1, Does.Contain("\"LeaseId\":\"LTEST654987\""));
        }

        [Test]
        public void When_LoggingWithMessageTemplateWithoutMatchingMetadata_Should_NullReplaceFormatMessageTemplate()
        {
            _logger.Log(
                LogLevel.Info,
                "This is a log message that includes LeaseId: {LeaseId}",
                metadata: new Dictionary<string, object> { ["LeaseeId"] = "LTEST654987" }
            );

            var message1 = InMemorySink.InMemorySink.Pop();

            Assert.That(
                message1,
                Does.Contain("This is a log message that includes LeaseId: (null)")
            );
            Assert.That(message1, Does.Contain("\"LeaseeId\":\"LTEST654987\""));
        }

        [Test]
        public void When_LoggingWithMessageTemplateWithMultipleReplacements_Should_FormatMessageTemplate()
        {
            _logger.Log(
                LogLevel.Info,
                "LeaseId: {LeaseId}, StoreId: {StoreId}, StoreName: {StoreName}",
                metadata: new Dictionary<string, object>
                {
                    ["LeaseId"] = "LTEST654987",
                    ["StoreId"] = "STEST1234"
                }
            );

            var message1 = InMemorySink.InMemorySink.Pop();

            Assert.That(
                message1,
                Does.Contain("LeaseId: LTEST654987, StoreId: STEST1234, StoreName: (null)")
            );
            Assert.That(
                message1,
                Does.Contain("\"LogDetails\":{\"LeaseId\":\"LTEST654987\",\"StoreId\":\"STEST1234\"}")
            );
        }

        [Test]
        public void When_SubmittingSameScopeMultipleTimes_Should_RespectInnerMostScopeOrderForMessages()
        {
            using (_logger.BeginScope(new Dictionary<string, object> { ["LeaseId"] = "LTEST654987" }))
            {
                _logger.Log(LogLevel.Info, "Log without declaring scope should include scope");
                _logger.Log(
                    LogLevel.Info,
                    "Declare scope again and they should both be in the log",
                    metadata: new Dictionary<string, object> { ["LeaseId"] = "LTEST654989" }
                );
            }

            var message1 = InMemorySink.InMemorySink.Pop();
            var message2 = InMemorySink.InMemorySink.Pop();

            Assert.That(message1, Does.Contain("\"LeaseId\":\"LTEST654987\""));
            Assert.That(message2, Does.Contain("\"LeaseId\":\"LTEST654989\""));
        }

        [Test]
        public void When_SubmittingScopeAsJsonString_Should_IncludeJsonStringInLogDetails()
        {
            _logger.Log(
                LogLevel.Info,
                "Declare scope again and they should both be in the log",
                metadata: new Dictionary<string, object>
                {
                    ["Address"] = "{\"Line1\":\"123 Test Drive\"}"
                }
            );

            var message1 = InMemorySink.InMemorySink.Pop();
            Assert.That(
                message1,
                Does.Contain("\"Address\":\"{\\\"Line1\\\":\\\"123 Test Drive\\\"}\"")
            );
        }

        [Test]
        public void When_SubmittingScopeAsObject_Should_IncludeLegitJsonForScopeInLogDetails()
        {
            _logger.Log(
                LogLevel.Info,
                "Declare scope {@Address} anon type and it should be in the LogDetails",
                metadata: new Dictionary<string, object>
                {
                    [
                        "@Address"
                    ] = new
                    {
                        Line1 = "123 Test Drive",
                        City = "YouWish",
                        IsActive = true,
                        InnerObj = new { InnerValue = "TestInnerValue" }
                    }
                }
            );

            var message1 = InMemorySink.InMemorySink.Pop();

            Assert.That(
                message1,
                Does.Contain(
                    "\"Message\":\"Declare scope {\\\"Line1\\\":\\\"123 Test Drive\\\",\\\"City\\\":\\\"YouWish\\\",\\\"IsActive\\\":true,\\\"InnerObj\\\":{\\\"InnerValue\\\":\\\"TestInnerValue\\\"}} anon type and it should be in the LogDetails\""
                )
            );
            Assert.That(
                message1,
                Does.Contain(
                    "\"Address\":{\"Line1\":\"123 Test Drive\",\"City\":\"YouWish\",\"IsActive\":true,\"InnerObj\":{\"InnerValue\":\"TestInnerValue\"}}"
                )
            );
        }

        [Test]
        public void When_NonStringScalarTypeIsUsedInMetadata_Should_BeInJsonFormatOfTypeInLogDetails()
        {
            _logger.Log(
                LogLevel.Info,
                "Declare scope again and they should both be in the log",
                metadata: new Dictionary<string, object>
                {
                    ["NumberOfThings"] = 33244,
                    ["DidThisSucceed"] = true
                }
            );

            var message1 = InMemorySink.InMemorySink.Pop();

            Assert.That(message1, Does.Contain("\"NumberOfThings\":33244"));
            Assert.That(message1, Does.Contain("\"DidThisSucceed\":true"));
        }
    }
}
