using System.Collections.Concurrent;

namespace Nexus.Logging;

/// <summary>
///     Optimization to preserve message template key extraction.
/// </summary>
internal struct FormattedLogMessage
{
    private const string NullMessage = "[null]";

    // To prevent eating up all the memory
    private const int MaxCachedFormatters = 1024;
    private static int _count;
    private static readonly ConcurrentDictionary<string, LogMessageFormatter> _formatters = new();
    private readonly LogMessageFormatter _formatter;
    private readonly IDictionary<string, object> _values;

    /// <summary>
    ///     Count of the cached template items.
    /// </summary>
    internal int Count => _count;

    /// <summary>
    ///     Formatter for the current message template.
    /// </summary>
    internal LogMessageFormatter Formatter => _formatter;

    /// <summary>
    ///     Get or add the formatter associated with the FormattedLogMessage.
    /// </summary>
    /// <param name="message">Log message with or without templated parameters.</param>
    /// <param name="values">Values to use for substituting the templated parameters.</param>
    public FormattedLogMessage(string message, IDictionary<string, object> values)
    {
        if (string.IsNullOrWhiteSpace(message)) message = NullMessage;

        if (!_formatters.TryGetValue(message, out _formatter))
        {
            if (_count <= MaxCachedFormatters)
                _formatter = _formatters.GetOrAdd(message, f =>
                {
                    Interlocked.Increment(ref _count);
                    return new LogMessageFormatter(message);
                });
            else
                _formatter = new LogMessageFormatter(message);
        }

        _values = values;
    }

    /// <summary>
    ///     Executes the formatter to populate the template with values.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _formatter.Format(_values);
    }
}