using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Nexus.Logging.Contract.Tests;

/// <summary>
///     This is here to provide a test for the build to run to make the build :happyface:
/// </summary>
public class DummyTestToMakeBuildHappy
{
    [Test]
    public void IMakeTheBuildHappy()
    {
        var logger = new DummyLogger<DummyTestToMakeBuildHappy>();
        Assert.That(logger, Is.Not.Null);
    }
}

public class DummyLogger<T> : ILogger<T>
{
    public IDisposable BeginScope(IDictionary<string, object> scopes)
    {
        return null;
    }

    public void Log(LogLevel level, string message, Exception exception = null,
        IDictionary<string, object> metaData = null)
    {
    }
}