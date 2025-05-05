using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Logging.Tests
{
    /// <summary>
    /// Provides an in memory logger implementation to show pipeline interactions and enable unit testing of the ProgLeasing.System.Logging components.
    /// This is an example of how the Microsoft.Extensions.Logging pipeline works when dealing with scoping from IExternalScopeProvider.
    /// Also shows how the ILoggerFactory -> ILoggerProvider setup the ILogger.
    /// </summary>
    public sealed class InMemoryLogger : Microsoft.Extensions.Logging.ILogger, IDisposable
    {
        private static readonly Queue<LogData> _logMessages = new Queue<LogData>();
        private readonly Action<LogData> WriteLogMessage = _logMessages.Enqueue;

        // Bound by the ILoggerProvider
        internal IExternalScopeProvider ScopeProvider { get; set; }

        // Since IExternalScopeProvider is being used this should not even get called
        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        // Not filtering levels for now
        public bool IsEnabled(LogLevel logLevel) => true;

        // Implementation of ILogger.Log
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logBuilder = new StringBuilder();
            logBuilder.Append("{");
            logBuilder.AppendFormat("\"Message\":\"{0}\"", formatter(state, exception));
            GetScopeInformation(logBuilder);
            logBuilder.Append("}");
            WriteLogMessage(new LogData
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = logBuilder.ToString(),
                Exception = exception
            });
        }

        public void Dispose()
        {
            _logMessages.Clear();
        }


        public static LogData PopLog()
        {
            return _logMessages.Dequeue();
        }

        private void GetScopeInformation(StringBuilder stringBuilder)
        {
            var scopeProvider = ScopeProvider;
            if (scopeProvider != null)
            {
                scopeProvider.ForEachScope((scope, state) =>
                {
                    state.Append((stringBuilder.Length > 0 ? "," : string.Empty));
                    state.Append(scope);
                }, (stringBuilder));
            }
        }
    }

    public class LogData
    {
        public LogLevel LogLevel { get; set; }

        public EventId EventId { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
