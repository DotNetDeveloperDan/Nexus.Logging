

using Nexus.Logging.Contract;

namespace Nexus.Logging.Configuration;

/// <summary>
///     Logger configuration options.
/// </summary>
public class LoggerOptions
{
    /// <summary>
    ///     Global minimum log level.
    /// </summary>
    public LogLevel? MinimumLevel { get; set; }

    /// <summary>
    ///     Override log levels based on <see cref="ILogger{TCategoryName}" />.
    /// </summary>
    public Dictionary<string, LogLevel?> Filters { get; init; }

    /// <summary>
    ///     Options for each defined <see cref="LoggerTarget" />.
    /// </summary>
    public IList<LoggerTargetOptions> Targets { get; init; }

    /// <summary>
    ///     Global output format for each <see cref="LoggerTarget" />.
    /// </summary>
    public string OutputTemplate { get; set; } = "";
}