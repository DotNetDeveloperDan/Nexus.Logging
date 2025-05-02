using Nexus.Logging.Contract;
using Serilog.Events;

namespace Nexus.Logging.Serilog;

public static class LogLevelExtensions
{
    /// <summary>
    ///     Convert the Serilog LogEventLevel to a Contract LogLevel.
    /// </summary>
    /// <param name="logEventLevel"></param>
    /// <returns></returns>
    public static LogLevel ToContractLogLevel(this LogEventLevel logEventLevel)
    {
        return logEventLevel switch
        {
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Info,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Warning => LogLevel.Warn,
            LogEventLevel.Fatal => LogLevel.Fatal,
            _ => LogLevel.Info
        };
    }

    /// <summary>
    ///     Convert a Contract LogLevel to a Serilog LogEventLevel.
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public static LogEventLevel ToSerilogLogLevel(this LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Info => LogEventLevel.Information,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Warn => LogEventLevel.Warning,
            LogLevel.Fatal => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}