using NUnit.Framework;

namespace Nexus.Logging.Serilog.Tests;

public class MessageScopeExtractorTests
{
    [Test]
    public void When_SequenceValueWithRawKvp_Should_ReturnMessageScopeWithElement()
    {
        var logEvent = LogEventData.InfoEventWithNestedScopes();
        logEvent.DestructureNestedScopes();

        Assert.That(logEvent.Properties, Does.ContainKey("NonStandard"));
        Assert.That(
            logEvent.Properties["NonStandard"].ToString(),
            Is.EqualTo("\"ShouldBeInLogDetails\""));
    }
}