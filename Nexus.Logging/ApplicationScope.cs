using Nexus.Logging.Configuration;

namespace Nexus.Logging;

/// <summary>
///     Holder for the configured global application scope properties to optimize usage.
/// </summary>
public class ApplicationScope
{
    public ApplicationScope(ApplicationScopeOptions options)
    {
        Options = options;
        Scope = new LoggerScope(options.ToDictionary());
    }

    /// <summary>
    ///     Configured options for ApplicationScope.
    /// </summary>
    public ApplicationScopeOptions Options { get; }

    /// <summary>
    ///     Current ApplicationScope packaged into the usable LoggerScope.
    /// </summary>
    public LoggerScope Scope { get; }
}