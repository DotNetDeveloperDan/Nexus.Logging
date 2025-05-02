using Serilog.Events;
using Serilog.Formatting.Json;

namespace Nexus.Logging.Serilog;

internal static class JsonValueFormatterExtensions
{
    /// <summary>
    ///     Write a <see cref="LogEvent" /> Property as Json.
    /// </summary>
    /// <param name="jsonValueFormatter"></param>
    /// <param name="logEvent">The log event that contains the property and value</param>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="textWriter">Writer that the result should be written to</param>
    internal static void WriteLogPropertyAsJson(this JsonValueFormatter jsonValueFormatter, LogEvent logEvent,
        string propertyName, TextWriter textWriter)
    {
        JsonValueFormatter.WriteQuotedJsonString(propertyName, textWriter);
        textWriter.Write(JsonFormat.Delimiter);
        jsonValueFormatter.Format(logEvent.GetPropertyValue(propertyName), textWriter);
    }
}