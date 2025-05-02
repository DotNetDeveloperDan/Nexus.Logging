using Serilog.Events;

namespace Nexus.Logging.Serilog;

/// <summary>
///     Extracts the scopes from Serlog into a <see cref="MessageScope" /> so they can be dealt with in KVP fashion.
/// </summary>
internal static class MessageScopeExtractor
{
    /// <summary>
    ///     Extract nested scopes from the provided <see cref="LogEventPropertyValue" />.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static MessageScope ExtractNestedScopes(LogEventPropertyValue value)
    {
        var messageScope = new MessageScope();
        Visit(value, messageScope);
        return messageScope;
    }

    /// <summary>
    ///     Support the visitor for the types expected based on usage.
    /// </summary>
    /// <param name="value">The value to visit.</param>
    /// <param name="messageScope"></param>
    /// <returns></returns>
    private static bool Visit(LogEventPropertyValue value, MessageScope messageScope)
    {
        return value switch
        {
            null => throw new ArgumentNullException(nameof(value)),
            ScalarValue sv => VisitScalarValue(sv, messageScope),
            SequenceValue sequenceValue => VisitSequenceValue(sequenceValue, messageScope),
            _ => false
        };
    }

    /// <summary>
    ///     Extract the <see cref="ScalarValue" /> type.
    /// </summary>
    /// <param name="scalar"></param>
    /// <param name="messageScope"></param>
    /// <returns></returns>
    private static bool VisitScalarValue(ScalarValue scalar, MessageScope messageScope)
    {
        if (scalar == null) throw new ArgumentNullException(nameof(scalar));
        SaveValue(scalar.Value, messageScope);
        return false;
    }

    /// <summary>
    ///     Extract the <see cref="SequenceValue" /> type.
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    private static bool VisitSequenceValue(SequenceValue sequence, MessageScope messageScope)
    {
        switch (sequence)
        {
            case null:
                throw new ArgumentNullException(nameof(sequence));
        }

        foreach (var t in sequence.Elements) Visit(t, messageScope);
        return false;
    }

    /// <summary>
    ///     Save the value.
    /// </summary>
    /// <param name="input"></param>
    private static void SaveValue(object? input, MessageScope messageScope)
    {
        switch (input)
        {
            // If there is a null value there is no way to determine a property name
            case null:
                return;
            case string str when IsKVP(str):
                var (key, value) = ExtractKeyValue(str);
                messageScope.AddValue(key, value);
                break;
            case string str:
                messageScope.AddValue(str, str);
                break;
            case KeyValuePair<string, string> kvp:
                messageScope.AddValue(kvp.Key, kvp.Value);
                break;
        }
    }

    /// <summary>
    ///     Determine if the string input is in the format of a KeyValuePair.ToString() value.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static bool IsKVP(string input)
    {
        return input.StartsWith('[') && input.LastIndexOf(']') == input.Length - 1 && input.IndexOf(',') > 0;
    }

    /// <summary>
    ///     Extract the KVP format associated with the KVP.ToString() method:
    ///     <code>[Key, Value]</code>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static (string Key, string Value) ExtractKeyValue(string input)
    {
        input = input.Substring(1, input.Length - 2);
        var indexOfFirstComma = input.IndexOf(',');
        var key = input[..indexOfFirstComma];
        var value = input[(indexOfFirstComma + 1)..];
        return (key.Trim(), value.Trim());
    }
}