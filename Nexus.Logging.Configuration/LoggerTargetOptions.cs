namespace Nexus.Logging.Configuration;

/// <summary>
///     Target configuration items.
/// </summary>
public class LoggerTargetOptions
{
    /// <summary>
    ///     The provider that will handle the logging target.
    ///     <para>The name of the provider as declared in <see cref="IConfigureLoggerProvider.ProviderName" /></para>
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    ///     <see cref="LoggerTarget" /> that the configuration is being defined for.
    /// </summary>
    public LoggerTarget? Type { get; set; }

    /// <summary>
    ///     Target specific override for the <see cref="LoggerTarget" />.
    /// </summary>
    public string OutputTemplate { get; set; }

    /// <summary>
    ///     Pass through arguments for configuring the <see cref="LoggerTarget" /> in the
    ///     <seealso cref="IConfigureLoggerProvider" />.
    /// </summary>
    public IDictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
}