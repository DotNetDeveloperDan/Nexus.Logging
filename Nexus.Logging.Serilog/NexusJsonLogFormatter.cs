using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Nexus.Logging.Serilog;

/// <summary>
///     Format the log message to Json.
/// </summary>
public class NexusJsonLogFormatter(JsonValueFormatter valueFormatter = null) : ITextFormatter
{
    /// <summary>
    ///     These are LogEvent Properties that should NOT be included in LogDetails since they are already in the log data.
    /// </summary>
    private static readonly string[] ExcludeFromLogDetails =
    [
        LogProperty.ApplicationName,
        LogProperty.Environment,
        LogProperty.InstanceId,
        LogProperty.MachineName,
        LogProperty.UserName,
        LogProperty.CorrelationId,
        LogProperty.ParentCorrelationId,
        LogProperty.Sequence,
        LogProperty.RequestId,
        LogProperty.SpanId,
        LogProperty.TraceId,
        LogProperty.RequestPath,
        LogProperty.State,
        LogProperty.SourceContext,
        LogProperty.ParentId,
        LogProperty.ActionId,
        LogProperty.ActionName
    ];

    private readonly JsonValueFormatter _valueFormatter = valueFormatter ?? new JsonValueFormatter(null);

    /// <summary>
    ///     Entrance point from the Logger.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="output"></param>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        FormatEvent(logEvent, output, _valueFormatter);
    }

    /// <summary>
    ///     Format the <see cref="LogEvent" /> into the required Json format.
    /// </summary>
    /// <param name="logEvent">Incoming event to log.</param>
    /// <param name="output">Writer from Sinks.</param>
    /// <param name="valueFormatter">Formatter to help with formatting <see cref="LogProperty" /> to valid Json</param>
    public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
    {
        switch (logEvent)
        {
            case null:
                throw new ArgumentNullException(nameof(logEvent));
        }

        switch (output)
        {
            case null:
                throw new ArgumentNullException(nameof(output));
        }

        if (valueFormatter != null)
        {
            logEvent.DestructureNestedScopes();

            output.Write(JsonFormat.StartObject);

            JsonFormat.WriteJsonPropertyAndValue(LogProperty.TimeStamp,
                logEvent.Timestamp.UtcDateTime.ToString("o"), output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.ApplicationName, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.Environment, output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.Type, LogProperty.Event, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.InstanceId, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.UserName, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.StackId, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.ParentCorrelationId, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.CorrelationId, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.Sequence, output);

            output.Write(JsonFormat.Comma);
            valueFormatter.WriteLogPropertyAsJson(logEvent, LogProperty.MachineName, output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.LogLevel,
                logEvent.Level.ToContractLogLevel().ToString().ToUpperInvariant(), output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.Message, logEvent.RenderMessage(), output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.Exception, logEvent.Exception?.Message ?? string.Empty,
                output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.TargetSite,
                logEvent.Exception?.TargetSite?.Name ?? string.Empty, output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.InnerExMessage,
                logEvent.Exception?.InnerException?.Message ?? string.Empty, output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.InnerExStackTrace,
                logEvent.Exception?.InnerException?.StackTrace ?? string.Empty, output);

            output.Write(JsonFormat.Comma);
            JsonFormat.WriteJsonPropertyAndValue(LogProperty.StackTrace,
                logEvent.Exception?.StackTrace ?? string.Empty, output);

            // destructure the LogDetails section
            var append = false;
            output.Write(JsonFormat.Comma);
            JsonValueFormatter.WriteQuotedJsonString(LogProperty.LogDetails, output);
            output.Write(JsonFormat.Delimiter);
            output.Write(JsonFormat.StartObject);
            foreach (var prop in logEvent.Properties.Where(w => !ExcludeFromLogDetails.Contains(w.Key)))
            {
                output.Write(append ? JsonFormat.Comma : string.Empty);
                valueFormatter.WriteLogPropertyAsJson(logEvent, prop.Key, output);
                append = true;
            }

            output.Write(JsonFormat.EndObject);

            output.WriteLine(JsonFormat.EndObject);
        }
        else
        {
            throw new ArgumentNullException(nameof(valueFormatter));
        }
    }
}