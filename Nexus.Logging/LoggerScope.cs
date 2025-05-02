using System.Collections;
using System.Text;

namespace Nexus.Logging;

/// <summary>
///     Provides the ability to serialize and maintain structured logging for libraries that aren't able to natively
///     ToString() as Console or Debug loggers.
/// </summary>
public sealed class LoggerScope(IDictionary<string, object> scopes) : IReadOnlyList<KeyValuePair<string, object>>
{
    private readonly KeyValuePair<string, object>[] _scopes = scopes.ToArray();

    /// <summary>
    ///     Forces JSON like data structure for logging providers that rely on ToString() to retrieve scope properties.
    ///     <para>When capable the Enumerator should be used to retrieve each KVP scope property.</para>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var t in _scopes)
            sb.AppendFormat("{0}\"{1}\":\"{2}\"", sb.Length > 0 ? "," : string.Empty, t.Key, t.Value);

        return sb.ToString();
    }

    #region IReadOnlyList Impl

    /// <summary>
    ///     Number of scope properties that are present.
    /// </summary>
    public int Count => _scopes.Length;

    /// <summary>
    ///     Indexer for scope properties.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public KeyValuePair<string, object> this[int index] =>
        _scopes.Length < index
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : new KeyValuePair<string, object>(_scopes[index].Key, _scopes[index].Value);

    /// <summary>
    ///     Enumerator for scope properties.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        for (var i = 0; i < _scopes.Length; i++) yield return this[i];
    }

    /// <summary>
    ///     Enumerator for scope properties.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}