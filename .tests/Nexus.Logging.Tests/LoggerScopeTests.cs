using System.Collections.Generic;
using NUnit.Framework;

namespace Nexus.Logging.Tests;

public class LoggerScopeTests
{
    [Test]
    public void When_LoggerScopeCreated_Should_ProperlyFormatLoggerScopeToString()
    {
        var ls = new LoggerScope(LoggerScopeTestData.FakeScopes);

        var result = ls.ToString();

        Assert.That(
            result,
            Is.EqualTo(
                "\"TestKey1\":\"TestValue1\",\"TestKey2\":\"TestValue2\",\"TestKey3\":\"{ TestAnon1 = Yes, TestAnon2 = True }\""
            )
        );
    }

    [Test]
    public void When_LoggerScopesRetrievedByEnumerator_Should_ContainLoggerScopesInEnumerator()
    {
        var loggerScope = new LoggerScope(LoggerScopeTestData.FakeScopes);

        var enumerator = loggerScope.GetEnumerator();
        while (enumerator.MoveNext())
            Assert.That(
                LoggerScopeTestData.FakeScopes.ContainsKey(enumerator.Current.Key),
                Is.True,
                $"Expected scope key '{enumerator.Current.Key}' to be present."
            );
    }
}

public static class LoggerScopeTestData
{
    public static IDictionary<string, object> FakeScopes =>
        new Dictionary<string, object>
        {
            ["TestKey1"] = "TestValue1",
            ["TestKey2"] = "TestValue2",
            ["TestKey3"] = new { TestAnon1 = "Yes", TestAnon2 = true }
        };
}