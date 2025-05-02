namespace Nexus.Logging.Serilog;

/// <summary>
///     Holder for destructuring Serilog scopes that were not treated as first class scopes aka LogEventPropertyValues.
/// </summary>
public class MessageScope
{
    /// <summary>
    ///     Named scopes with associated values.
    /// </summary>
    private readonly IDictionary<string, object> _scopes = new Dictionary<string, object>();

    /// <summary>
    ///     Add the scope value.
    ///     <para>If the key already has a value it will concatenate the new value to preserve previous values.</para>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddValue(string key, object value)
    {
        if (_scopes.TryGetValue(key, out var currentValue)
            && currentValue is string stringVal
            && !string.IsNullOrWhiteSpace(stringVal))
            _scopes[key] = string.Concat(stringVal, ",", value);
        else
            _scopes[key] = value;
    }

    /// <summary>
    ///     Get the value of a named scope.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object Get(string name)
    {
        return _scopes.TryGetValue(name, out var value) ? value : string.Empty;
    }

    /// <summary>
    ///     Enumerator for scopes that were extracted from the LogEvent.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, object>> GetMessageScopes()
    {
        foreach (var scope in _scopes) yield return scope;
    }
}