using Microsoft.Extensions.Logging;

namespace Nexus.Logging.Configuration;

/// <summary>
///     Used to configure a provider for the logger.
///     <para>Microsoft.Extensions.Logging, Serilog, NLog...</para>
/// </summary>
public interface IConfigureLoggerProvider
{
    /// <summary>
    ///     The name of the logging provider.
    ///     <para>
    ///         Corresponds with the <see cref="LoggerTargetOptions.Provider" /> to control target to provider relationship
    ///         during build.
    ///     </para>
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    ///     Used to configure the <see cref="IConfigureLoggerProvider" /> with the specified logging target options.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to configure the provider with.</param>
    /// <param name="options">Logger configuration options.</param>
    /// <param name="scopeOptions">Application specific configuration options.</param>
    void Configure(ILoggingBuilder builder, LoggerOptions options, ApplicationScopeOptions scopeOptions);
}