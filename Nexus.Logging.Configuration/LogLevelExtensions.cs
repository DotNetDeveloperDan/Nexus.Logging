using Microsoft.Extensions.Logging;

namespace Nexus.Logging.Configuration;

public static class LogLevelExtensions
{
    /// <summary>
    ///     Extension to convert from <see cref="Contract.LogLevel" /> to
    ///     <seealso cref="Microsoft.Extensions.Logging.LogLevel" />.
    /// </summary>
    /// <param name="internalLogLevel"></param>
    /// <returns></returns>
    public static LogLevel ConvertLogLevel(this Contract.LogLevel internalLogLevel)
    {
        return internalLogLevel switch
        {
            Contract.LogLevel.Debug => LogLevel.Debug,
            Contract.LogLevel.Error => LogLevel.Error,
            Contract.LogLevel.Fatal => LogLevel.Critical,
            Contract.LogLevel.Info => LogLevel.Information,
            Contract.LogLevel.Warn => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}