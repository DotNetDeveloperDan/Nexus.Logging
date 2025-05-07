using System.Collections.Generic;
using NUnit.Framework;

namespace Nexus.Logging.Tests;

public class FormattedMessageTests
{
    [TestCase("This should have found {testkey} and {something}", 2)]
    [TestCase("This should have no keys found", 0)]
    [TestCase("This should have not found testkey} or {something", 0)]
    [TestCase("This should have found {{testkey} and {something}}", 2)]
    public void When_FormattedMessageCreatedWithKeys_Should_ExtractKeys(string message, int keyCount)
    {
        var formattedMessage = new FormattedLogMessage(message, FormattedMessageTestData.FakeMetadata);

        Assert.That(formattedMessage.Formatter.MessageTemplate, Is.EqualTo(message));
        Assert.That(formattedMessage.Formatter.TemplateKeys.Length, Is.EqualTo(keyCount));
    }

    [Test]
    public void When_SameFormattedMessageCreatedMultipleTimes_Should_NotAddAnotherLogMessageFormatter()
    {
        const string Message = "This should have replaced {testkey} with {something}";
        var formattedMessage1 = new FormattedLogMessage(Message, FormattedMessageTestData.FakeMetadata);
        var expectedCount = formattedMessage1.Count;

        // first instance
        Assert.That(formattedMessage1.Formatter.MessageTemplate, Is.EqualTo(Message));
        Assert.That(formattedMessage1.Formatter.TemplateKeys.Length, Is.EqualTo(2));

        // second instance should reuse the cached formatter
        var formattedMessage2 = new FormattedLogMessage(Message, FormattedMessageTestData.FakeMetadata);
        Assert.That(formattedMessage2.Formatter.MessageTemplate, Is.EqualTo(Message));
        Assert.That(formattedMessage2.Count, Is.EqualTo(expectedCount));
    }
}

public static class FormattedMessageTestData
{
    public static IDictionary<string, object> FakeMetadata =>
        new Dictionary<string, object>
        {
            ["testkey"] = "replacedtestkey",
            ["something"] = "replacedsomething"
        };
}