using Serilog.Events;

namespace Nexus.Logging.Serilog;

/// <summary>
///     Extensions to help with LogEvents.
/// </summary>
public static class LogEventExtensions
{
    /// <summary>
    ///     Get the value of a LogEvent property as its string representation.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static LogEventPropertyValue GetPropertyValue(this LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var value)) return value;

        return new ScalarValue(string.Empty);
    }

    /// <summary>
    ///     Extract the provided <see cref="LogEventPropertyValue" /> as a <seealso cref="MessageScope" />.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static void DestructureNestedScopes(this LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(LogProperty.Scope, out var scope))
        {
            var extractedScope = MessageScopeExtractor.ExtractNestedScopes(scope);
            foreach (var es in extractedScope.GetMessageScopes())
                logEvent.AddPropertyIfAbsent(new LogEventProperty(es.Key, new ScalarValue(es.Value)));
            logEvent.RemovePropertyIfPresent(LogProperty.Scope);
        }
    }
}