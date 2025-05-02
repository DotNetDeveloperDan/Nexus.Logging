namespace Nexus.Logging.Configuration;

/// <summary>
///     The configuration for <see cref="LoggerOptions" /> was not valid.
/// </summary>
public class LoggerConfigurationException : Exception
{
    public LoggerConfigurationException(string message)
        : base(message)
    {
    }

    public LoggerConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}