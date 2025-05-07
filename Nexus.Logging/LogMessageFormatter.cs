using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nexus.Logging;

public class LogMessageFormatter
{
    private const string NullValue = "(null)";

    /// <summary>
    ///     Types that will be serialized using their respective ToString implementation.
    /// </summary>
    private static readonly HashSet<Type> _scalarTypes = new()
    {
        typeof(bool),
        typeof(char),
        typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
        typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
        typeof(string),
        typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
        typeof(Guid), typeof(Uri)
    };

    private static readonly Regex _pattern = new(@"{(.+?)}", RegexOptions.Compiled);

    public LogMessageFormatter(string messageTemplate)
    {
        MessageTemplate = messageTemplate;
        TemplateKeys = ExtractKeys(messageTemplate);
    }

    public string MessageTemplate { get; }

    public string[] TemplateKeys { get; }

    /// <summary>
    ///     Finds all the parameters within the message template that have a named {key} pattern.
    /// </summary>
    /// <param name="messageTemplate"></param>
    /// <returns></returns>
    private string[] ExtractKeys(string messageTemplate)
    {
        var matches = _pattern.Matches(messageTemplate);
        var keys = new string[matches.Count];
        for (var i = 0; i < matches.Count; i++) keys[i] = matches[i].Groups[1].Value;
        return keys;
    }

    /// <summary>
    ///     Replaces the message template keys with the
    ///     <param name="values">values</param>
    ///     provided.
    ///     <para>Key matches are case sensitive.</para>
    /// </summary>
    /// <param name="values">Key, Value to replace in the message template.</param>
    /// <returns></returns>
    public string Format(IDictionary<string, object> values)
    {
        if (TemplateKeys.Length == 0 || values == null || values.Count == 0) return MessageTemplate;
        var formatted = MessageTemplate;
        foreach (var t in TemplateKeys)
        {
            var value = values.ContainsKey(t) ? FormatObjectValue(values[t]) : NullValue;
            formatted = formatted.Replace("{" + t + "}", value);
        }

        return formatted;
    }

    /// <summary>
    ///     Format the <paramref name="value" /> based on the underlying type.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string FormatObjectValue(object value)
    {
        if (value == null) return NullValue;

        var type = value.GetType();

        if (_scalarTypes.Contains(type)) return value.ToString();

        // Always serialize anonymous types as Json as their ToString representation has odd formatting
        if (IsAnonymousType(type)) return JsonSerializer.Serialize(value);

        // Default serialization using ToString if it is overriden in the Type, otherwise serialize as Json
        return type.ToString() != value.ToString() ? value.ToString() : JsonSerializer.Serialize(value);
    }

    private static bool IsAnonymousType(Type type)
    {
        return type == null
            ? throw new ArgumentNullException("type")
            :
            // HACK: The only way to detect anonymous types right now.
            Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
            && type.IsGenericType && type.Name.Contains("AnonymousType")
            && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
            && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }
}