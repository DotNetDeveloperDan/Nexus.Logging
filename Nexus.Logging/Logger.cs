namespace Nexus.Logging
{
    /// <summary>
    /// Generic logger implementation that wraps 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Logger<T> : Contract.ILogger<T>
    {
        /// <summary>
        /// Callback to invoke the <see cref="LogMessageFormatter"/> for the <seealso cref="FormattedLogMessage"/>.
        /// </summary>
        private static readonly Func<FormattedLogMessage, Exception, string> _messageFormatter = MessageFormatter;

        private readonly Microsoft.Extensions.Logging.ILogger<T> _baseLogger;
        private readonly ApplicationScope _appScope;

        public Logger(Microsoft.Extensions.Logging.ILogger<T> logger, ApplicationScope applicationScope)
        {
            _baseLogger = logger;
            _appScope = applicationScope;
        }

        /// <summary>
        /// Use to set parameters to become properties on the log message.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public IDisposable BeginScope(IDictionary<string, object> metadata)
        {
            return _baseLogger.BeginScope(new LoggerScope(metadata));
        }

        /// <summary>
        /// Call the underlying <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/> to log.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="metadata"></param>
        public void Log(Contract.LogLevel logLevel, string message, Exception exception = null, IDictionary<string, object> metadata = null)
        {
            var baseLevel = MapLevel(logLevel);
            if (!_baseLogger.IsEnabled(baseLevel))
            {
                return;
            }
            using (_baseLogger.BeginScope(_appScope.Scope))
            {
                IDisposable metadataScope = null;
                if (metadata != null && metadata.Count > 0)
                    metadataScope = BeginScope(metadata);

                _baseLogger.Log(baseLevel, 0, new FormattedLogMessage(message, metadata), exception, _messageFormatter);

                if (metadataScope != null)
                    metadataScope.Dispose();
            }
        }

        /// <summary>
        /// Convert <see cref="Contract.LogLevel"/> to <seealso cref="Microsoft.Extensions.Logging.LogLevel"/>.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        private static Microsoft.Extensions.Logging.LogLevel MapLevel(Contract.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Contract.LogLevel.Debug:
                    return Microsoft.Extensions.Logging.LogLevel.Debug;
                case Contract.LogLevel.Error:
                    return Microsoft.Extensions.Logging.LogLevel.Error;
                case Contract.LogLevel.Fatal:
                    return Microsoft.Extensions.Logging.LogLevel.Critical;
                case Contract.LogLevel.Info:
                    return Microsoft.Extensions.Logging.LogLevel.Information;
                case Contract.LogLevel.Warn:
                    return Microsoft.Extensions.Logging.LogLevel.Warning;
                default:
                    return Microsoft.Extensions.Logging.LogLevel.Information;
            }
        }

        /// <summary>
        /// Helper to invoke the <see cref="LogMessageFormatter"/> that formats the message with parameters.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="exception">The exception that was logged.
        /// <para>Not used in message but needed to satisfy <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}"/> contract.</para>
        /// </param>
        /// <returns></returns>
        private static string MessageFormatter(FormattedLogMessage message, Exception exception)
        {
            return message.ToString();
        }
    }
}
