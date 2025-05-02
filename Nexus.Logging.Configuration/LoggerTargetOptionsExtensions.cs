using System.ComponentModel;

namespace Nexus.Logging.Configuration;

public static class LoggerTargetOptionsExtensions
{
    /// <summary>
    ///     Get the value associated with the key or the specified default value when the key does not exist.
    /// </summary>
    /// <typeparam name="T">
    ///     The Type the result should be converted to based on the Type of the
    ///     <paramref name="defaultValue" />.
    /// </typeparam>
    /// <param name="input">The dictionary that should be searched with the <paramref name="key" />.</param>
    /// <param name="key">The key to use to search for a value in the dictionary.</param>
    /// <param name="defaultValue">
    ///     The value that should be returned when the <paramref name="key" /> does not exist or have a
    ///     value.
    /// </param>
    /// <returns></returns>
    public static T GetValueOrDefault<T>(this IDictionary<string, string> input, string key, T defaultValue)
    {
        if (input.TryGetValue(key, out var retrieved))
            if (!string.IsNullOrWhiteSpace(retrieved))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null) return (T)converter.ConvertFromString(retrieved);
            }

        return defaultValue;
    }
}