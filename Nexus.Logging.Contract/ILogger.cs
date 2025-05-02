namespace Nexus.Logging.Contract;

/// <summary>
///     Generic logger.
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Perform a logging operation.
    /// </summary>
    /// <param name="level">The logging level to use for the operation.</param>
    /// <param name="message">The preformatted log message.</param>
    /// <param name="exception">Include when an exception is present and should be included with the message.</param>
    /// <param name="metadata">Parameters for use for formatted log messages and adding to the log message scope.</param>
    void Log(LogLevel level, string message, Exception exception = null, IDictionary<string, object> metadata = null);

    /// <summary>
    ///     Set logging scope values that will be passed through the call stack and included with all log messages.
    /// </summary>
    /// <param name="metadata">Parameters to include with the log message scope.</param>
    /// <returns></returns>
    /// <example>
    ///     <code>
    /// using (var scope = ILogger.BeginScope(new Dictionary<string, object>
    ///             {
    ///             {"TransactionId", "C407A07A-767A-44E3-9DCC-0BFEAE3A7FB5"}
    ///             }))
    ///             {
    ///             ILogger.Log(LogLevel.Debug, "This message will include TransactionId scope");
    ///             // Any calls to other methods that include ILogger.Log calls will contain the TransactionId scope
    ///             }
    /// </code>
    /// </example>
    IDisposable BeginScope(IDictionary<string, object> metadata);
}