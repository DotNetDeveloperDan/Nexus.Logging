namespace Nexus.Logging.Configuration;

/// <summary>
///     Defines the Type for the logging target.
/// </summary>
public enum LoggerTarget
{
    Unknown,
    Console,
    Debug,
    RollingFile,

    /// <summary>
    ///     Example would be AppInsights, Splunk or anything that is pushed directly.
    /// </summary>
    Service
}