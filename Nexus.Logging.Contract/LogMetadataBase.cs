using System.Runtime.CompilerServices;

namespace Nexus.Logging.Contract;

/// <summary>
///     Extend to provide metadata objects with strongly typed properties.
/// </summary>
public abstract class LogMetadataBase : Dictionary<string, string>
{
    public new string this[string key]
    {
        get
        {
            TryGetValue(key, out var tempVal);
            return tempVal;
        }
        set => (this as Dictionary<string, string>)[key] = value;
    }

    /// <summary>
    ///     Retrieve a value for a specified key.
    ///     <para>Use in the property getter of implementation.</para>
    ///     <code>get { return GetValue(); }</code>
    /// </summary>
    /// <param name="key">
    ///     Key that should be used to retrieve the value. When used in getter the property name will auto
    ///     populate.
    /// </param>
    /// <returns></returns>
    public string GetValue([CallerMemberName] string key = "")
    {
        return this[key];
    }

    /// <summary>
    ///     Set the value for a specified key.
    ///     <para>Use in the property setter of implementation.</para>
    ///     <code>set { SetValue(value); }</code>
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="key">Key that should be used to save the value. When used in setter the property name will auto populate.</param>
    public void SetValue(string value, [CallerMemberName] string key = "")
    {
        this[key] = value;
    }
}