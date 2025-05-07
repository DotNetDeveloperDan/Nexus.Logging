using System.Collections.Generic;
using NUnit.Framework;

namespace Nexus.Logging.Configuration.Tests;

public class LoggerTargetOptionsTests
{
    private readonly IDictionary<string, string> _testArgs = new Dictionary<string, string>
    {
        { "FileCount", "10" },
        { "PathFormat", "\\logs\\testing.log" },
        { "FileSize", "10000000" }
    };

    [TestCase("FileCount", 15, 10, Description = "Match should be found with return type int")]
    [TestCase("fileCount", 15, 15, Description = "Default should be returned with return type int")]
    [TestCase("PathFormat", @"\defaultlogs\log.log", @"\logs\testing.log",
        Description = "Match should be found with return type string")]
    [TestCase("FileSize", 15000000L, 10000000L, Description = "Match should be found with return type long")]
    [TestCase("Unmatched", true, true,
        Description = "Default should be returned since key does not exist with return type bool")]
    public void When_GettingValueFromVals_Should_GetValueWhenExistsOrDefaultWhenNotExists<T>(string key, T defaultValue,
        T expected)
    {
        var result = _testArgs.GetValueOrDefault(key, defaultValue);

        // verify type
        Assert.That(result, Is.InstanceOf<T>());

        // verify value
        Assert.That(result, Is.EqualTo(expected));
    }
}