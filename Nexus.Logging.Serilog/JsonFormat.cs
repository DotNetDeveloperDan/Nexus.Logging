using Serilog.Formatting.Json;

namespace Nexus.Logging.Serilog;

/// <summary>
///     Used to help format key value pair as a Json representation.
/// </summary>
internal static class JsonFormat
{
    internal const string Comma = ",";
    internal const string Delimiter = ":";
    internal const string StartObject = "{";
    internal const string EndObject = "}";

    /// <summary>
    /// </summary>
    /// <param name="formatter"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="textWriter"></param>
    internal static void WriteJsonPropertyAndValue(string name, string value, TextWriter textWriter)
    {
        JsonValueFormatter.WriteQuotedJsonString(name, textWriter);
        textWriter.Write(Delimiter);
        JsonValueFormatter.WriteQuotedJsonString(value, textWriter);
    }
}